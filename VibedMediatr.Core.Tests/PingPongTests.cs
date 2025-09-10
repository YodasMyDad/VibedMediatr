using MediatR;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using VibedMediatr;

public sealed record Ping(string Message) : IRequest<string>;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}

public class PingPongTests
{
    [Fact]
    public async Task Send_Returns_Handler_Result()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        var result = await mediator.Send(new Ping("x"));
        result.Should().Be("Pong: x");
    }

    [Fact]
    public async Task Send_With_CancellationToken_Returns_Handler_Result()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource();
        var result = await mediator.Send(new Ping("test"), cts.Token);
        result.Should().Be("Pong: test");
    }

    [Fact]
    public async Task Send_Caches_Handler_For_Performance()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();

        // First call - builds cache
        var result1 = await mediator.Send(new Ping("first"));
        result1.Should().Be("Pong: first");

        // Second call - uses cached executor
        var result2 = await mediator.Send(new Ping("second"));
        result2.Should().Be("Pong: second");
    }
}
