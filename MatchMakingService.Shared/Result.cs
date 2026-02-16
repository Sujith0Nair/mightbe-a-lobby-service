namespace MatchMakingService.Shared;

public class Result<T>
{   
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccessful { get; }

    private Result(bool isSuccessful, T? value, string? errorMessage)
    {
        IsSuccessful = isSuccessful;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Fail(string errorMessage, T? value = default) => new(false, value, errorMessage);
}

public class Result
{
    public string? ErrorMessage { get; }
    public bool IsSuccessful { get; }

    private Result(bool isSuccessful, string? errorMessage)
    {
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;;
    }

    public static Result Success() => new(true, null);
    public static Result Fail(string errorMessage) => new(false, errorMessage);
}