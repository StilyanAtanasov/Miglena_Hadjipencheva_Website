using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using System.Reflection;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.EntityConstraints.LegalDocumentNode;

namespace MHAuthorWebsite.Core;

public class LegalDocumentsService : ILegalDocumentsService
{
    protected readonly IApplicationRepository Repository;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private const string SeedResourcePrefix = "MHAuthorWebsite.Core.SeedData.LegalDocuments.";
    private static readonly IReadOnlyDictionary<LegalDocumentType, string> SeedResourceNames = new Dictionary<LegalDocumentType, string>
    {
        [LegalDocumentType.PrivacyPolicy] = "privacy-policy.v1.json",
        [LegalDocumentType.TermsOfService] = "terms-of-service.v1.json"
    };

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
                .Where(d => d.DocumentType == pending.DocumentType).MaxBy(d => d.Version);

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
        LegalDocumentSeedDto seed = LoadSeed(type);
        LegalDocumentNodeDto[] nodes = seed.Nodes
            .Select((node, index) => CreateNode(index + 1, node.Title.Trim(), NormalizeParagraphs(node.Paragraphs)))
            .ToArray();

        return new LegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = type,
            Version = Math.Max(1, seed.Version),
            Title = seed.Title.Trim(),
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
                n.Title.Length is >= TitleMinLength and <= TitleMaxLength &&
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

    private LegalDocumentSeedDto LoadSeed(LegalDocumentType type)
    {
        if (!SeedResourceNames.TryGetValue(type, out string? fileName) || string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException($"No legal document seed file is configured for {type}.");

        string resourceName = $"{SeedResourcePrefix}{fileName}";
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Missing embedded legal document seed: {resourceName}.");

        LegalDocumentSeedDto? seed = JsonSerializer.Deserialize<LegalDocumentSeedDto>(stream, _jsonOptions);
        if (seed is null || string.IsNullOrWhiteSpace(seed.Title))
            throw new InvalidOperationException($"Invalid legal document seed content: {resourceName}.");

        if (seed.Nodes.Count == 0)
            throw new InvalidOperationException($"Legal document seed has no nodes: {resourceName}.");

        foreach (LegalDocumentSeedNodeDto node in seed.Nodes)
        {
            if (string.IsNullOrWhiteSpace(node.Title))
                throw new InvalidOperationException($"Legal document seed contains a node without title: {resourceName}.");

            if (node.Paragraphs.Count == 0 || node.Paragraphs.All(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Legal document seed contains a node without content: {resourceName}.");
        }

        return seed;
    }

    private static string NormalizeParagraphs(ICollection<string> paragraphs)
        => string.Join("\n\n", paragraphs.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));

    private sealed class LegalDocumentSeedDto
    {
        public int Version { get; set; }

        public string Title { get; set; } = null!;

        public List<LegalDocumentSeedNodeDto> Nodes { get; set; } = new();
    }

    private sealed class LegalDocumentSeedNodeDto
    {
        public string Title { get; set; } = null!;

        public List<string> Paragraphs { get; set; } = new();
    }
}
