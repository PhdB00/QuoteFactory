using MediatR;

namespace QuoteFetcher.Application.Abstractions.Messaging;

public interface IStreamQueryHandler<TRequest, TResponse>
    : IStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamQuery<TResponse>;