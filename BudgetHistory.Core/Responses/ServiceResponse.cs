namespace BudgetHistory.Core.Services.Responses
{
    public class ServiceResponse<T> : ServiceResponse where T : class
    {
        public T Value { get; set; }

        public static ServiceResponse<T> Success(T value, string message = "") => new() { IsSuccess = true, Value = value, Message = message };

        public new static ServiceResponse<T> Failure(string errorMessage) => new() { IsSuccess = false, Message = errorMessage, Value = default };
    }

    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResponse Success(string message = "") => new() { IsSuccess = true, Message = message };

        public static ServiceResponse Failure(string errorMessage) => new() { IsSuccess = false, Message = errorMessage };
    }
}