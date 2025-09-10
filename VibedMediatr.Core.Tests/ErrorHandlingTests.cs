using MediatR;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using VibedMediatr;

public class ErrorHandlingTests
{
    [Fact]
    public async Task Send_Null_Request_Throws_ArgumentNullException()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.Send<string>(null!));

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.Send(null!));
    }

    [Fact]
    public async Task Send_Request_Without_Handler_Throws_InvalidOperationException()
    {
        var services = new ServiceCollection()
            .AddVibedMediatr(typeof(Ping).Assembly)
            .BuildServiceProvider();

        var mediator = services.CreateScope().ServiceProvider.GetRequiredService<IMediator>();

        // Create a request type that exists but has no handler registered
        var request = new UnregisteredRequest();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.Send(request));

        exception.Message.Should().Contain("No handler registered");
        exception.Message.Should().Contain(nameof(UnregisteredRequest));
    }

    // Note: Testing invalid request types is complex due to compile-time type safety.
    // The Mediator validates request types at runtime, so this test would require reflection.
}

public sealed record UnregisteredRequest() : IRequest;
