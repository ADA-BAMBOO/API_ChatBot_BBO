using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            var result = await DbSet.AddAsync(entity);
            // Entity Framework will set the Chatid after the entity is added
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<BboChathistory> GetLatestChatHistoryAsync(int userId, string message)
    {
        return await DbSet
            .Where(x => x.Userid == userId && x.Message == message)
            .OrderByDescending(x => x.Sentat)
            .FirstOrDefaultAsync();
    }
}
