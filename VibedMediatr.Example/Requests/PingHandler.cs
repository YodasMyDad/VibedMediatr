using MediatR;
using VibedMediatr.Example.Requests;

namespace VibedMediatr.Example.Requests;

/// <summary>
/// Handler for the Ping request.
/// </summary>
public sealed class PingHandler : IRequestHandler<Ping, string>
{
    /// <summary>
    /// Handles the ping request and returns a pong response.
    /// </summary>
    /// <param name="request">The ping request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pong response.</returns>
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}
