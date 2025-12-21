namespace MHAuthorWebsite.Data.Shared.Filters.Contracts;

public interface IFilter<T> where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}