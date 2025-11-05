namespace AuthService.Domain.Abstractions;

public readonly struct Error(string Code, string Message)
{
    public string Code { get; } = Code; public string Message { get; } = Message; public override string ToString()
    {
        return $"{Code}: {Message}";
    }
}
public readonly struct Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    private Result(bool s, Error? e) { IsSuccess = s; Error = e; }
    public static Result Success()
    {
        return new(true, null);
    }

    public static Result Failure(string c, string m)
    {
        return new(false, new Error(c, m));
    }
}
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    private Result(bool s, T? v, Error? e) { IsSuccess = s; Value = v; Error = e; }
    public static Result<T> Success(T v)
    {
        return new(true, v, null);
    }

    public static Result<T> Failure(string c, string m)
    {
        return new(false, default, new Error(c, m));
    }
}
