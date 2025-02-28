using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatBot.API.Reponsitory;

public class FilterReponsitory : GenericReponsitory<BboFilter>, IFilterReponsitory
{


    public FilterReponsitory(YourDbContext _dbContext) : base(_dbContext)
    {
    }

    public override async Task<List<BboFilter>> GetAllAsync()
    {
        return await DbSet.OrderByDescending(x=> x.Displayorder).ToListAsync();
    }

    public override async Task<BboFilter> GetAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<bool> AddEntity(BboFilter entity)
    {
        try
        {
            var result = await DbSet.AddAsync(entity);
            // Entity Framework will set the Chatid after the entity is added
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}
