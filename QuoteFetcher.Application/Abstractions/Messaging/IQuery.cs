using MediatR;

namespace QuoteFetcher.Application.Abstractions.Messaging;

public interface IQuery : IRequest;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

