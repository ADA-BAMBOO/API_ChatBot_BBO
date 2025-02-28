namespace ChatBot.API.Interface;

public interface IGenericReponsitory<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllAsyncPaged(int pageIndex, int pageSize);
    Task<T> GetAsync(int id);
    Task<T> GetFirstOrDefaultAsync(int id);
    Task<bool> AddEntity(T entity);
    Task<bool> UpdateEntity(T entity);
    Task<bool> DeleteEntity(int id);

}
