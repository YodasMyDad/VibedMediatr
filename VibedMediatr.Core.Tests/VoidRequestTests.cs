using MediatR;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using VibedMediatr;

public sealed record DoThing() : IRequest;

public sealed class DoThingHandler : IRequestHandler<DoThing>
{
    public Task Handle(DoThing request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

public class VoidRequestTests
{
    [Fact]
    public async Task Send_VoidRequest_Completes()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(DoThing).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new DoThing());
        // If we get here without exception, the test passes
    }

    [Fact]
    public async Task Send_VoidRequest_With_CancellationToken_Completes()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(DoThing).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource();
        await mediator.Send(new DoThing(), cts.Token);
        // If we get here without exception, the test passes
    }

    [Fact]
    public async Task Send_VoidRequest_Cancellation_Works()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(DoThing).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await mediator.Send(new DoThing(), cts.Token));
    }
}
