namespace ChatBot.API.Interface;

public interface IUnitOfWork
{
    IUserReponsitory userReponsitory { get; }
    IRoleReponsitory roleReponsitory { get; }
    IChatHistoryReponsitory chatHistoryReponsitory { get; }
    IFeedbackReponsitory feedbackReponsitory { get; }

    Task CompleteAsync();
}
