using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Reponsitory;

public class RoleReponsitory : GenericReponsitory<BboRole>, IRoleReponsitory
{
    public RoleReponsitory(YourDbContext _dbContext) : base(_dbContext)
    {
    }

    public override Task<List<BboRole>> GetAllAsync()
    {
        return DbSet.OrderByDescending(x => x.Id).ToListAsync();
    }

    public override async Task<BboRole> GetAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<bool> AddEntity(BboRole entity)
    {
        try
        {
            await DbSet.AddAsync(entity);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    //public override async Task<bool> UpdateEntity(BboRole entity)
    //{
    //    try
    //    {
    //        var result = await DbSet.FirstOrDefaultAsync(x => x.Id == entity.Id);
    //        if(result != null)
    //        {

    //            result.Lastactive = DateTime.Now;
    //            return true;

    //        }
    //        else{ 
    //        return false;
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception(ex.Message);
    //    }
    //}
}
