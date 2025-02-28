using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Reponsitory;

public class FeedbackReponsitory : GenericReponsitory<BboFeedback>, IFeedbackReponsitory
{
    public FeedbackReponsitory(YourDbContext _dbContext) : base(_dbContext)
    {
    }

    public override Task<List<BboFeedback>> GetAllAsync()
    {
        return DbSet.OrderByDescending(x => x.Id).ToListAsync();
    }

    public override async Task<BboFeedback> GetAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<bool> AddEntity(BboFeedback entity)
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

   
}
