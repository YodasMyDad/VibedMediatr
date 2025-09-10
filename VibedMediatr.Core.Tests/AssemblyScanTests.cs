using MediatR;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using VibedMediatr;

// Request and handler in a separate "external" assembly simulation
public sealed record ExternalPing(string Message) : IRequest<string>;

public sealed class ExternalPingHandler : IRequestHandler<ExternalPing, string>
{
    public Task<string> Handle(ExternalPing request, CancellationToken cancellationToken)
        => Task.FromResult($"External Pong: {request.Message}");
}

public class AssemblyScanTests
{
    [Fact]
    public async Task Additional_Assembly_Scan_Finds_Handlers()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(/* entry assembly */ typeof(AssemblyScanTests).Assembly,
                             /* extra */ typeof(ExternalPing).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        var result = await mediator.Send(new ExternalPing("hi"));
        result.Should().Be("External Pong: hi");
    }

    [Fact]
    public async Task Default_Assembly_Scan_Works()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly) // Need to explicitly include the test assembly
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        var result = await mediator.Send(new Ping("default"));
        result.Should().Be("Pong: default");
    }

    [Fact]
    public async Task Null_Assembly_In_Params_Is_Ignored()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly, null!, typeof(ExternalPing).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();

        // Both handlers should be available
        var result1 = await mediator.Send(new Ping("test"));
        result1.Should().Be("Pong: test");

        var result2 = await mediator.Send(new ExternalPing("test"));
        result2.Should().Be("External Pong: test");
    }
}
