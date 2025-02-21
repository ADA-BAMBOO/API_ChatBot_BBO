using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Reponsitory;

public class ChatHistoryReponsitory : GenericReponsitory<BboChathistory>, IChatHistoryReponsitory
{
    public ChatHistoryReponsitory(YourDbContext _dbContext) : base(_dbContext)
    {
    }

    public override Task<List<BboChathistory>> GetAllAsync()
    {
        return base.GetAllAsync();
    }

    public override async Task<BboChathistory> GetAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Chatid == id);
    }

    public override async Task<bool> AddEntity(BboChathistory entity)
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
