namespace rbbl.buildingblocks.Results;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    protected Result(bool success, string? error) { IsSuccess = success; Error = error; }
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T? Value { get; }
    private Result(bool success, string? error, T? value) : base(success, error) { Value = value; }
    public static Result<T> Success(T value) => new(true, null, value);
    public static new Result<T> Failure(string error) => new(false, error, default);
}
