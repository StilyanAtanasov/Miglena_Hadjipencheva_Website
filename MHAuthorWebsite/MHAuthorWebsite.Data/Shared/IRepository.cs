using System.Linq.Expressions;

namespace MHAuthorWebsite.Data.Shared;

public interface IRepository : IDisposable, IAsyncDisposable
{
    // READ-ONLY (non-tracking)
    IQueryable<T> AllReadonly<T>() where T : class;
    IQueryable<T> WhereReadonly<T>(Expression<Func<T, bool>> predicate) where T : class;

    // Tracking (read + write)
    IQueryable<T> All<T>() where T : class;
    IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class;

    // Fetching single entity
    Task<T?> GetByIdAsync<T>(object id) where T : class;
    Task<T?> FindByExpressionAsync<T>(
        Expression<Func<T, bool>> expression,
        params Expression<Func<T, object>>[] includes) where T : class;

    // Fetching multiple entities
    Task<List<T>> FindAllByExpressionAsync<T>(
        Expression<Func<T, bool>> expression,
        params Expression<Func<T, object>>[] includes) where T : class;

    // Aggregates
    Task<bool> CheckExpressionAsync<T>(
        Expression<Func<T, bool>> expression,
        params Expression<Func<T, object>>[] includes) where T : class;

    Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
    Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : class;

    // CRUD
    Task AddAsync<T>(T entity) where T : class;
    Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class;
    void Update<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;
    void DeleteRange<T>(IEnumerable<T> entities) where T : class;

    // State management
    void Attach<T>(T entity) where T : class;
    void Detach<T>(T entity) where T : class;

    // Persistence
    Task<int> SaveChangesAsync();

    // Pagination
    Task<List<T>> GetPagedAsync<T>(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false) where T : class;

    // Transactional
    Task ExecuteInTransactionAsync(Func<Task> action);
}

