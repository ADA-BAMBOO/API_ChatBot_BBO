using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Reponsitory;

public class UserReponsitory : GenericReponsitory<BboUser>, IUserReponsitory
{
    public UserReponsitory(YourDbContext _dbContext) : base(_dbContext)
    {
    }

    public override Task<List<BboUser>> GetAllAsync()
    {
        return base.GetAllAsync();
    }

    public override async Task<BboUser> GetAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Telegramid == id);
    }

    public override async Task<BboUser> GetFirstOrDefaultAsync(int telegramId)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Telegramid == telegramId);
    }

    public override async Task<bool> AddEntity(BboUser entity)
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

    public override async Task<bool> UpdateEntity(BboUser entity)
    {
        try
        {
            var result = await DbSet.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (result != null)
            {

                result.Lastactive = DateTime.Now;
                return true;

            }
            else
            {
                return false;
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
