namespace MHAuthorWebsite.Data.Shared;

public class ApplicationRepository : EfRepository, IApplicationRepository
{
    public ApplicationRepository(ApplicationDbContext context) : base(context) { }
}
