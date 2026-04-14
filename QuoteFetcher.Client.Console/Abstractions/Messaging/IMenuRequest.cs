using QuoteFetcher.Application.Abstractions;
using MediatR;

namespace QuoteFetcher.Abstractions.Messaging;

public interface IMenuRequest : IRequest;

public interface IMenuRequest<TResponse> : IRequest<Result<TResponse>>;

