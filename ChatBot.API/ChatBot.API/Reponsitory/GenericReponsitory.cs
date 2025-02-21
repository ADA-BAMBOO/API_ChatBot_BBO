using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Reponsitory;

public class GenericReponsitory<T> : IGenericReponsitory<T> where T : class
{
    protected YourDbContext dbContext;
    internal DbSet<T> DbSet { get; set; }
    public GenericReponsitory(YourDbContext _dbContext)
    {
        this.dbContext = _dbContext;
        this.DbSet = this.dbContext.Set<T>();

    }

    public virtual Task<List<T>> GetAllAsync()
    {
        return DbSet.ToListAsync();
    }

    public virtual Task<T> GetAsync(int id)
    {
        throw new NotImplementedException();
    }

    public virtual Task<T> GetFirstOrDefaultAsync(int id)
    {
        throw new NotImplementedException();
    }
    public virtual Task<bool> AddEntity(T entity)
    {
        throw new NotImplementedException();
    }

    public virtual Task<bool> DeleteEntity(int id)
    {
        throw new NotImplementedException();
    }

    public virtual Task<bool> UpdateEntity(T entity)
    {
        throw new NotImplementedException();
    }

}
