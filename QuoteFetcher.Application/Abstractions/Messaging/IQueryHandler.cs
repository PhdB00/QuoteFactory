using MediatR;

namespace QuoteFetcher.Application.Abstractions.Messaging;

public interface IQueryHandler<in TRequest> : IRequestHandler<TRequest>
    where TRequest : IQuery;

public interface IQueryHandler<TRequest, TResponse> 
    : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IQuery<TResponse>;

