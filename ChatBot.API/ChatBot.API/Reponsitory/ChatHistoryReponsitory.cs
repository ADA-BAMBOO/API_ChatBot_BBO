using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatBot.API.Reponsitory;

public class ChatHistoryReponsitory : GenericReponsitory<BboChathistory>, IChatHistoryReponsitory
{
    private readonly ILogger<ChatHistoryReponsitory> _logger;

    public ChatHistoryReponsitory(YourDbContext dbContext) : base(dbContext)
    {
    }

    public Task<List<BboChathistory>> GetAllAsyncPaged(int pageIndex, int pageSize)
    {
        return DbSet.OrderByDescending(x => x.Chatid)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
    }

    public override Task<List<BboChathistory>> GetAllAsync()
    {
        return DbSet.OrderByDescending(x => x.Chatid).ToListAsync();
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
            _logger.LogError(ex, "Lỗi khi thêm chat history");
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

    public async Task<DateTime?> GetLastMessageTimeAsync(int userId)
    {
        var lastMessage = await DbSet
            .Where(x => x.Userid == userId)
            .OrderByDescending(x => x.Sentat)
            .FirstOrDefaultAsync();

        return lastMessage?.Sentat;
    }
}
