using ChatBot.API.Interface;
using ChatBot.API.Models;

namespace ChatBot.API.Reponsitory;

public class UnitOfWorkReponsitory : IUnitOfWork
{
    public IUserReponsitory userReponsitory { get; private set; }
    public IRoleReponsitory roleReponsitory { get; private set; }
    public IChatHistoryReponsitory chatHistoryReponsitory { get; private set; }
    public IFeedbackReponsitory feedbackReponsitory { get; private set; }
    public IFilterReponsitory filterReponsitory { get; private set; }
    public ICreditReponsitory creditReponsitory { get; private set; }

    private readonly YourDbContext yourDbContext;

    public UnitOfWorkReponsitory(YourDbContext _yourDbContext)
    {
        this.yourDbContext = _yourDbContext;
        userReponsitory = new UserReponsitory(yourDbContext);
        roleReponsitory = new RoleReponsitory(yourDbContext);
        feedbackReponsitory = new FeedbackReponsitory(yourDbContext);
        chatHistoryReponsitory = new ChatHistoryReponsitory(yourDbContext);
        filterReponsitory = new FilterReponsitory(yourDbContext);
        creditReponsitory = new CreditReponsitory(yourDbContext);

    }
    public async Task CompleteAsync()
    {
        this.yourDbContext.SaveChanges();
    }
}
