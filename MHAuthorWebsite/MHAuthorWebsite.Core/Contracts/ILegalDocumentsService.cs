using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Contracts;

public interface ILegalDocumentsService
{
    Task<LegalDocumentDto> GetLatestDocumentReadonlyAsync(LegalDocumentType type);

    Task<ICollection<LegalDocumentDto>> GetLatestDocumentsReadonlyAsync();

    Task<ICollection<LegalDocumentDto>> GetPendingLatestDocumentsReadonlyAsync(string userId);

    Task<bool> HasUserAcceptedLatestDocumentsAsync(string userId);

    Task<ServiceResult> AcceptLatestDocumentsAsync(string userId);
}
