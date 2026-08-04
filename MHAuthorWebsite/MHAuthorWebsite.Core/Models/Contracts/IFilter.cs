namespace MHAuthorWebsite.Core.Models.Contracts;

public interface IFilter<T> where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}