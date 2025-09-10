using MediatR;

namespace VibedMediatr.Example.Requests;

/// <summary>
/// A simple ping request that returns a pong response.
/// </summary>
/// <param name="Message">The message to include in the response.</param>
public sealed record Ping(string Message) : IRequest<string>;
