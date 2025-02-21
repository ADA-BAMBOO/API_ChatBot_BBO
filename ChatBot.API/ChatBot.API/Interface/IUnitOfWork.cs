namespace ChatBot.API.Interface;

public interface IUnitOfWork
{
    IUserReponsitory userReponsitory { get; }
    IRoleReponsitory roleReponsitory { get; }
    IChatHistoryReponsitory chatHistoryReponsitory { get; }

    Task CompleteAsync();
}
