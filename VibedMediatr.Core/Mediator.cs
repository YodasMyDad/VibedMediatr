using System.Collections.Concurrent;
using System.Reflection;
using MediatR;

namespace VibedMediatr;

/// <summary>
/// Internal mediator implementation with caching for performance.
/// </summary>
internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    // Cache: request type -> executor delegate
    private static readonly ConcurrentDictionary<Type, HandlerExecutor> _executorCache = new();

    private delegate Task<object?> HandlerExecutor(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken);

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    public Mediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <summary>
    /// Sends a request that returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var executor = _executorCache.GetOrAdd(request.GetType(), BuildExecutorForResponseType);
        var result = await executor(_serviceProvider, request, cancellationToken).ConfigureAwait(false);
        return (TResponse)result!;
    }

    /// <summary>
    /// Sends a request that does not return a response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var executor = _executorCache.GetOrAdd(request.GetType(), BuildExecutorForVoidType);
        return executor(_serviceProvider, request, cancellationToken);
    }

    private static HandlerExecutor BuildExecutorForResponseType(Type requestType)
    {
        var irequestInterface = requestType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                               ?? throw new InvalidOperationException($"Request type {requestType} does not implement IRequest<T>.");

        var responseType = irequestInterface.GetGenericArguments()[0];

        var method = typeof(Mediator).GetMethod(nameof(ExecuteWithResponse), BindingFlags.NonPublic | BindingFlags.Static)!
                                      .MakeGenericMethod(requestType, responseType);
        return (HandlerExecutor)Delegate.CreateDelegate(typeof(HandlerExecutor), method);
    }

    private static HandlerExecutor BuildExecutorForVoidType(Type requestType)
    {
        if (!typeof(IRequest).IsAssignableFrom(requestType))
            throw new InvalidOperationException($"Request type {requestType} does not implement IRequest.");

        var method = typeof(Mediator).GetMethod(nameof(ExecuteVoid), BindingFlags.NonPublic | BindingFlags.Static)!
                                      .MakeGenericMethod(requestType);
        return (HandlerExecutor)Delegate.CreateDelegate(typeof(HandlerExecutor), method);
    }

    private static async Task<object?> ExecuteWithResponse<TRequest, TResponse>(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var handler = serviceProvider.GetService(typeof(IRequestHandler<TRequest, TResponse>)) as IRequestHandler<TRequest, TResponse>
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name} -> {typeof(TResponse).Name}.");

        var result = await handler.Handle((TRequest)request, cancellationToken).ConfigureAwait(false);
        return result; // boxed as object
    }

    private static async Task<object?> ExecuteVoid<TRequest>(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        var handler = serviceProvider.GetService(typeof(IRequestHandler<TRequest>)) as IRequestHandler<TRequest>
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}.");

        await handler.Handle((TRequest)request, cancellationToken).ConfigureAwait(false);
        return null;
    }
}
