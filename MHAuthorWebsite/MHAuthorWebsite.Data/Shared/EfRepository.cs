using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Data.Shared;

public abstract class EfRepository : IRepository
{
    private readonly DbContext _context;

    protected EfRepository(DbContext context) => _context = context;

    public void Dispose() => _context.Dispose();
    public async ValueTask DisposeAsync() => await _context.DisposeAsync();

    protected DbSet<T> DbSet<T>() where T : class => _context.Set<T>();

    // READ-ONLY (no tracking)
    public IQueryable<T> AllReadonly<T>() where T : class =>
        DbSet<T>().AsNoTracking();

    public IQueryable<T> WhereReadonly<T>(Expression<Func<T, bool>> predicate) where T : class =>
        DbSet<T>().Where(predicate).AsNoTracking();

    // TRACKING queries
    public IQueryable<T> All<T>() where T : class =>
        DbSet<T>();

    public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class =>
        DbSet<T>().Where(predicate);

    // FETCHING
    public async Task<T?> GetByIdAsync<T>(object id) where T : class =>
        await DbSet<T>().FindAsync(id);

    public async Task<T?> FindByExpressionAsync<T>(
        Expression<Func<T, bool>> expression,
        bool ignoreFilters = false,
        params Expression<Func<T, object>>[] includes) where T : class
    {
        IQueryable<T> query = DbSet<T>();
        foreach (var include in includes)
            query = query.Include(include);

        if (ignoreFilters) query = query.IgnoreQueryFilters();

        return await query.FirstOrDefaultAsync(expression);
    }

    public async Task<List<T>> FindAllByExpressionAsync<T>(
        Expression<Func<T, bool>> expression,
        params Expression<Func<T, object>>[] includes) where T : class
    {
        IQueryable<T> query = DbSet<T>();
        foreach (var include in includes)
            query = query.Include(include);

        return await query.Where(expression).ToListAsync();
    }

    // AGGREGATES
    public async Task<bool> CheckExpressionAsync<T>(
        Expression<Func<T, bool>> expression,
        params Expression<Func<T, object>>[] includes) where T : class
    {
        IQueryable<T> query = DbSet<T>();
        foreach (var include in includes)
            query = query.Include(include);

        return await query.AnyAsync(expression);
    }

    public async Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate) where T : class =>
        await DbSet<T>().AnyAsync(predicate);

    public async Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : class =>
        predicate == null
            ? await DbSet<T>().CountAsync()
            : await DbSet<T>().CountAsync(predicate);

    // CRUD
    public async Task AddAsync<T>(T entity) where T : class =>
        await DbSet<T>().AddAsync(entity);

    public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class =>
        await DbSet<T>().AddRangeAsync(entities);

    public void Update<T>(T entity) where T : class =>
        DbSet<T>().Update(entity);

    public void Delete<T>(T entity) where T : class =>
        DbSet<T>().Remove(entity);

    public void DeleteRange<T>(IEnumerable<T> entities) where T : class =>
        DbSet<T>().RemoveRange(entities);

    public void Attach<T>(T entity) where T : class =>
        DbSet<T>().Attach(entity);

    public void Detach<T>(T entity) where T : class =>
        _context.Entry(entity).State = EntityState.Detached;

    // PERSISTENCE
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    // PAGINATION
    public async Task<List<T>> GetPagedAsync<T>(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false) where T : class
    {
        IQueryable<T> query = DbSet<T>();

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = descending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // TRANSACTION SUPPORT
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
