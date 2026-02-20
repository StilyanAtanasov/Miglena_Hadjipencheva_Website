using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.EntityConstraints.LegalDocumentNode;

namespace MHAuthorWebsite.Core;

public class LegalDocumentsService : ILegalDocumentsService
{
    protected readonly IApplicationRepository Repository;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public LegalDocumentsService(IApplicationRepository repository)
        => Repository = repository;

    public async Task<LegalDocumentDto> GetLatestDocumentReadonlyAsync(LegalDocumentType type)
    {
        await EnsureDefaultDocumentsAsync();

        LegalDocument document = (await Repository
            .WhereReadonly<LegalDocument>(d => d.DocumentType == type)
            .OrderByDescending(d => d.Version)
            .FirstOrDefaultAsync())!;

        return Map(document);
    }

    public async Task<ICollection<LegalDocumentDto>> GetLatestDocumentsReadonlyAsync()
    {
        await EnsureDefaultDocumentsAsync();

        LegalDocumentType[] allTypes = Enum.GetValues<LegalDocumentType>();
        List<LegalDocumentDto> result = new();

        foreach (LegalDocumentType type in allTypes)
        {
            LegalDocument? document = await Repository
                .WhereReadonly<LegalDocument>(d => d.DocumentType == type)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();
            if (document is not null) result.Add(Map(document));
        }

        return result;
    }

    public async Task<ICollection<LegalDocumentDto>> GetPendingLatestDocumentsReadonlyAsync(string userId)
    {
        ICollection<LegalDocumentDto> latestDocuments = await GetLatestDocumentsReadonlyAsync();

        List<LegalDocumentDto> pending = new();
        foreach (LegalDocumentDto latest in latestDocuments)
        {
            bool accepted = await Repository
                .WhereReadonly<UserLegalAgreement>(a =>
                    a.UserId == userId &&
                    a.DocumentType == latest.DocumentType &&
                    a.DocumentVersion == latest.Version)
                .AnyAsync();
            if (!accepted) pending.Add(latest);
        }

        return pending;
    }

    public async Task<bool> HasUserAcceptedLatestDocumentsAsync(string userId)
        => !(await GetPendingLatestDocumentsReadonlyAsync(userId)).Any();

    public async Task<ServiceResult> AcceptLatestDocumentsAsync(string userId)
    {
        ICollection<LegalDocumentDto> pendingDocuments = await GetPendingLatestDocumentsReadonlyAsync(userId);
        if (!pendingDocuments.Any()) return ServiceResult.Ok();

        LegalDocumentType[] types = pendingDocuments.Select(d => d.DocumentType).ToArray();
        LegalDocument[] latestDocuments = await Repository
            .Where<LegalDocument>(d => types.Contains(d.DocumentType))
            .ToArrayAsync();

        foreach (LegalDocumentDto pending in pendingDocuments)
        {
            LegalDocument? doc = latestDocuments
                .Where(d => d.DocumentType == pending.DocumentType)
                .OrderByDescending(d => d.Version)
                .FirstOrDefault();
            if (doc is null) continue;

            UserLegalAgreement agreement = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DocumentId = doc.Id,
                DocumentType = doc.DocumentType,
                DocumentVersion = doc.Version,
                AgreedOn = DateTime.UtcNow
            };

            await Repository.AddAsync(agreement);
        }

        await Repository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    protected async Task EnsureDefaultDocumentsAsync()
    {
        LegalDocumentType[] allTypes = Enum.GetValues<LegalDocumentType>();
        bool hasChanges = false;
        foreach (LegalDocumentType type in allTypes)
        {
            bool hasAny = await Repository
                .WhereReadonly<LegalDocument>(d => d.DocumentType == type)
                .AnyAsync();

            if (hasAny) continue;

            LegalDocument defaultDocument = CreateDefaultDocument(type);
            await Repository.AddAsync(defaultDocument);
            hasChanges = true;
        }

        if (hasChanges)
            await Repository.SaveChangesAsync();
    }

    protected LegalDocument CreateDefaultDocument(LegalDocumentType type)
    {
        LegalDocumentNodeDto[] nodes = type switch
        {
            LegalDocumentType.PrivacyPolicy => new[]
            {
                CreateNode(1, "Администратор на лични данни", "Администратор на лични данни е Миглена Хаджипенчева. За контакт относно защита на данни: stilyan2008@gmail.com."),
                CreateNode(2, "Какви данни обработваме", "Обработваме данни за профил, поръчки, комуникация и сигурност на платформата."),
                CreateNode(3, "Вашите права", "Имате право на достъп, корекция, изтриване, ограничаване, преносимост и възражение по GDPR.")
            },
            _ => new[]
            {
                CreateNode(1, "Предмет", "Тези Общи условия уреждат използването на уебсайта и свързаните услуги."),
                CreateNode(2, "Права и задължения", "Потребителят използва сайта добросъвестно и не злоупотребява с функционалностите."),
                CreateNode(3, "Отговорност", "Собственикът полага грижа за наличност и сигурност, но не носи отговорност при форсмажорни обстоятелства.")
            }
        };

        return new LegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = type,
            Version = 1,
            Title = type == LegalDocumentType.PrivacyPolicy ? "Политика за поверителност" : "Общи условия",
            NodesJson = JsonSerializer.Serialize(nodes, _jsonOptions),
            CreatedOn = DateTime.UtcNow,
            AdminId = null
        };
    }

    private LegalDocumentDto Map(LegalDocument document)
    {
        LegalDocumentNodeDto[] nodes = DeserializeNodes(document.NodesJson);
        return new LegalDocumentDto
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            Version = document.Version,
            Title = document.Title,
            CreatedOn = document.CreatedOn,
            Nodes = nodes.OrderBy(n => n.Number).ToArray()
        };
    }

    protected LegalDocumentNodeDto[] DeserializeNodes(string nodesJson)
    {
        LegalDocumentNodeDto[]? nodes = JsonSerializer.Deserialize<LegalDocumentNodeDto[]>(nodesJson, _jsonOptions);
        if (nodes is null) return Array.Empty<LegalDocumentNodeDto>();

        return nodes
            .Where(n =>
                n.Number > 0 &&
                !string.IsNullOrWhiteSpace(n.Title) &&
                n.Title.Length >= TitleMinLength &&
                n.Title.Length <= TitleMaxLength &&
                !string.IsNullOrWhiteSpace(n.ContentDelta) &&
                n.ContentDelta.Length <= ContentDeltaMaxLength)
            .OrderBy(n => n.Number)
            .ToArray();
    }

    private static LegalDocumentNodeDto CreateNode(int number, string title, string text)
        => new()
        {
            Number = number,
            Title = title,
            ContentDelta = JsonSerializer.Serialize(new
            {
                ops = new[]
                {
                    new { insert = $"{text}\n" }
                }
            })
        };
}
