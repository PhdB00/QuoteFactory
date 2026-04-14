using MediatR;

namespace QuoteFetcher.Application.Abstractions.Messaging;

public interface IStreamQuery<out TResponse> : IStreamRequest<TResponse>;