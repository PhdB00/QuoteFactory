using System.Diagnostics.CodeAnalysis;

namespace QuoteFetcher.Application.Abstractions;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    
    public Error Error { get; }

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    
    public static Result<TValue> Failure<TValue>(Exception error) => 
        new(default, false, new Error(error.HResult.ToString(), error.Message));
    
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
}

public sealed class Result<TValue>(TValue? value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    [NotNull]
    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);	
}