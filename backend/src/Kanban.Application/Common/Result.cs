namespace Kanban.Application.Common;

public class Result<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];

    public static Result<T> Ok(T value) => new() { Success = true, Value = value };
    public static Result<T> Fail(IEnumerable<string> errors) => new() { Success = false, Errors = errors };
    public static Result<T> Fail(string error) => new() { Success = false, Errors = [error] };
}
