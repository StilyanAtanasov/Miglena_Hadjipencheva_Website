namespace MHAuthorWebsite.Data.Common;

public class ApplicationRepository : EfRepository, IApplicationRepository
{
    public ApplicationRepository(ApplicationDbContext context) : base(context) { }
}
