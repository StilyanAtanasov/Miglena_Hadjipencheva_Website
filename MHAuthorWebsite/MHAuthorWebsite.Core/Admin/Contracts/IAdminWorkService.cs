using MHAuthorWebsite.Core.Admin.Dto.Work;
using MHAuthorWebsite.Core.Common.Utils;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminWorkService
{
    Task<ServiceResult> AddWorkAsync(AddWorkDto model);

    Task<ServiceResult<EditWorkDto>> GetWorkForEditReadonlyAsync(Guid id);

    Task<ServiceResult> UpdateWorkAsync(EditWorkDto model);

    Task<ServiceResult> DeleteWorkAsync(Guid id);

    Task<ServiceResult> ToggleWorkPublicityAsync(Guid id);
}
