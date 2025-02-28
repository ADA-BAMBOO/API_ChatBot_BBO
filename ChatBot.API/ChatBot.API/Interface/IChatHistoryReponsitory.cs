using ChatBot.API.Models;

namespace ChatBot.API.Interface;

public interface IChatHistoryReponsitory : IGenericReponsitory<BboChathistory>
{
    Task<List<BboChathistory>> GetAllAsyncPaged(int pageIndex, int pageSize);
    Task<BboChathistory> GetLatestChatHistoryAsync(int userId, string message);
    Task<DateTime?> GetLastMessageTimeAsync(int userId);
}
