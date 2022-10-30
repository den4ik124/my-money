namespace BudgetHistory.Logging.Interfaces
{
    public interface ICustomLoggerFactory
    {
        CustomLogger CreateLogger<T>();
    }
}