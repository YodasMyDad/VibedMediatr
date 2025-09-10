// Assembly: VibedMediatr
// Namespace preserved for drop-in compatibility
namespace MediatR
{
    /// <summary>
    /// Marker interface for requests that return a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    public interface IRequest<out TResponse> { }

    /// <summary>
    /// Marker interface for requests that do not return a response.
    /// </summary>
    public interface IRequest { }

    /// <summary>
    /// Handler for requests that return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <typeparam name="TResponse">The type of response returned.</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles the request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response from handling the request.</returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Handler for requests that do not return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    public interface IRequestHandler<in TRequest>
        where TRequest : IRequest
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Minimal mediator interface to dispatch requests to their handlers.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Sends a request that returns a response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response expected.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response from the handler.</returns>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a request that does not return a response.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Send(IRequest request, CancellationToken cancellationToken = default);
    }
}
