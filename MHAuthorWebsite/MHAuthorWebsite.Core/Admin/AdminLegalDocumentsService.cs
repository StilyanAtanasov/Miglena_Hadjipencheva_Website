using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.LegalDocuments;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using System.Text.Json;
using LegalDocumentConstraints = MHAuthorWebsite.GCommon.EntityConstraints.LegalDocument;
using LegalDocumentNodeConstraints = MHAuthorWebsite.GCommon.EntityConstraints.LegalDocumentNode;

namespace MHAuthorWebsite.Core.Admin;

public class AdminLegalDocumentsService : LegalDocumentsService, IAdminLegalDocumentsService
{
    private readonly IApplicationRepository _repository;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AdminLegalDocumentsService(IApplicationRepository repository)
        : base(repository)
        => _repository = repository;

    public async Task<ICollection<AdminLegalDocumentSummaryDto>> GetLatestDocumentsSummaryReadonlyAsync()
    {
        ICollection<LegalDocumentDto> latest = await GetLatestDocumentsReadonlyAsync();
        return latest
            .Select(d => new AdminLegalDocumentSummaryDto
            {
                DocumentType = d.DocumentType,
                Title = d.Title,
                Version = d.Version,
                CreatedOn = d.CreatedOn
            })
            .OrderBy(d => (int)d.DocumentType)
            .ToArray();
    }

    public async Task<ServiceResult<LegalDocumentDto>> GetLatestDocumentForEditReadonlyAsync(LegalDocumentType type)
    {
        LegalDocumentDto document = await GetLatestDocumentReadonlyAsync(type);
        return ServiceResult<LegalDocumentDto>.Ok(document);
    }

    public async Task<ServiceResult<int>> SaveNewDocumentVersionAsync(AdminLegalDocumentUpdateDto dto, string adminId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length < LegalDocumentConstraints.TitleMinLength || dto.Title.Length > LegalDocumentConstraints.TitleMaxLength)
            return ServiceResult<int>.BadRequest(new() { [nameof(dto.Title)] = "Невалидно заглавие на документа." });

        LegalDocumentNodeDto[] nodes = dto.Nodes
            .Where(n => n.Number > 0)
            .OrderBy(n => n.Number)
            .ToArray();

        if (!nodes.Any())
            return ServiceResult<int>.BadRequest(new() { [nameof(dto.Nodes)] = "Добавете поне един раздел." });

        if (nodes.GroupBy(n => n.Number).Any(g => g.Count() > 1))
            return ServiceResult<int>.BadRequest(new() { [nameof(dto.Nodes)] = "Номерата на разделите трябва да са уникални." });

        foreach (LegalDocumentNodeDto node in nodes)
        {
            if (string.IsNullOrWhiteSpace(node.Title) || node.Title.Length < LegalDocumentNodeConstraints.TitleMinLength || node.Title.Length > LegalDocumentNodeConstraints.TitleMaxLength)
                return ServiceResult<int>.BadRequest(new() { [nameof(dto.Nodes)] = "Всеки раздел трябва да има валидно заглавие." });
            if (string.IsNullOrWhiteSpace(node.ContentDelta) || node.ContentDelta.Length > LegalDocumentNodeConstraints.ContentDeltaMaxLength)
                return ServiceResult<int>.BadRequest(new() { [nameof(dto.Nodes)] = "Всеки раздел трябва да има валидно съдържание." });
        }

        int currentVersion = await _repository
            .WhereReadonly<LegalDocument>(d => d.DocumentType == dto.DocumentType)
            .OrderByDescending(d => d.Version)
            .Select(d => d.Version)
            .FirstOrDefaultAsync();

        int nextVersion = currentVersion + 1;
        string nodesJson = JsonSerializer.Serialize(nodes, _jsonOptions);
        if (nodesJson.Length > LegalDocumentConstraints.NodesJsonMaxLength)
            return ServiceResult<int>.BadRequest(new() { [nameof(dto.Nodes)] = "Съдържанието е прекалено голямо." });

        LegalDocument newDocumentVersion = new()
        {
            Id = Guid.NewGuid(),
            DocumentType = dto.DocumentType,
            Version = nextVersion,
            Title = dto.Title.Trim(),
            NodesJson = nodesJson,
            CreatedOn = DateTime.UtcNow,
            AdminId = adminId
        };

        await _repository.AddAsync(newDocumentVersion);
        await _repository.SaveChangesAsync();

        return ServiceResult<int>.Ok(nextVersion);
    }
}
