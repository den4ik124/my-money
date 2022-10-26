namespace BudgetHistory.Core.Services.Responses
{
    public class ServiceResponse<T> : ServiceResponse where T : class
    {
        public T Value { get; set; }
    }

    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}