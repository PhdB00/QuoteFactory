using QuoteFetcher.Application.Abstractions;
using MediatR;

namespace QuoteFetcher.Abstractions.Messaging;

public interface IMenuRequestHandler<in TRequest> 
    : IRequestHandler<TRequest>
    where TRequest : IMenuRequest;

public interface IMenuRequestHandler<TRequest, TResponse> 
    : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IMenuRequest<TResponse>;
