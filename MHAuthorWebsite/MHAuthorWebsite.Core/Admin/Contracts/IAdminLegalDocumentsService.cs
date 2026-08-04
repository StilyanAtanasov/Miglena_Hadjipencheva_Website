using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.LegalDocuments;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminLegalDocumentsService
{
    Task<ICollection<AdminLegalDocumentSummaryDto>> GetLatestDocumentsSummaryReadonlyAsync();

    Task<ServiceResult<LegalDocumentDto>> GetLatestDocumentForEditReadonlyAsync(LegalDocumentType type);

    Task<ServiceResult<int>> SaveNewDocumentVersionAsync(AdminLegalDocumentUpdateDto dto, string adminId);
}
