using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Reponsitory;

public class CreditReponsitory : GenericReponsitory<BboCredit>, ICreditReponsitory
{
    public CreditReponsitory(YourDbContext _dbContext) : base(_dbContext)
    {
    }

    public override Task<List<BboCredit>> GetAllAsync()
    {
        return DbSet.OrderByDescending(x => x.Id).ToListAsync();
    }

    public override async Task<BboCredit> GetAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<BboCredit> GetFirstOrDefaultAsync(int telegramId)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == telegramId);
    }

    public override async Task<bool> AddEntity(BboCredit entity)
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

    public override async Task<bool> UpdateEntity(BboCredit entity)
    {
        try
        {
            var result = await DbSet.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (result != null)
            {
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
