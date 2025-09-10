# VibedMediatr

A tiny, secure, and efficient drop-in replacement for MediatR's most common usage pattern.

[![NuGet](https://img.shields.io/nuget/v/VibedMediatr.svg)](https://www.nuget.org/packages/VibedMediatr/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)

## Overview

VibedMediatr is a minimal implementation of the [Mediator pattern](https://en.wikipedia.org/wiki/Mediator_pattern) that focuses exclusively on the core request/handler pattern used by most MediatR applications. It's designed to be a drop-in replacement for apps that only need:

- `IRequest<TResponse>` and `IRequest` interfaces
- `IRequestHandler<TRequest, TResponse>` and `IRequestHandler<TRequest>` interfaces
- `IMediator.Send(...)` method

**Everything else is intentionally out of scope.** This keeps the codebase small, focused, and easy to understand.

## Key Features

✅ **Drop-in replacement** - Uses the same `MediatR` namespace and contracts  
✅ **Minimal surface area** - Only 3 interfaces and 1 extension method  
✅ **High performance** - Cached delegates avoid repeated reflection  
✅ **Secure by default** - Opt-in assembly scanning only  
✅ **Scoped mediator** - Avoids capturing the root service provider  
✅ **Full async support** - Cancellation tokens throughout  

## Installation

```bash
dotnet add package VibedMediatr
```

## Quick Start

### 1. Define a Request

```csharp
using MediatR;

public sealed record Ping(string Message) : IRequest<string>;
```

### 2. Create a Handler

```csharp
using MediatR;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}
```

### 3. Register with DI

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
// Scan entry assembly + any additional assemblies
builder.Services.AddVibedMediatr(typeof(Program).Assembly);

var app = builder.Build();
```

### 4. Use the Mediator

```csharp
public class IndexModel(IMediator mediator) : PageModel
{
    public string? Reply { get; private set; }

    public async Task OnGet()
    {
        Reply = await mediator.Send(new Ping("Hello World!"));
    }
}
```

## API Reference

### Interfaces

```csharp
namespace MediatR
{
    public interface IRequest<out TResponse> { }
    public interface IRequest { }

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<in TRequest>
        where TRequest : IRequest
    {
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        Task Send(IRequest request, CancellationToken cancellationToken = default);
    }
}
```

### Extension Methods

```csharp
namespace VibedMediatr
{
    public static class ServiceCollectionExtensions
    {
        // Scan only entry assembly
        public static IServiceCollection AddVibedMediatr(this IServiceCollection services);

        // Scan entry assembly + additional assemblies
        public static IServiceCollection AddVibedMediatr(this IServiceCollection services, params Assembly[] additionalAssemblies);
    }
}
```

## Performance

VibedMediatr uses a two-phase approach for optimal performance:

1. **Cold path** (first request of each type): Reflection is used to build a compiled delegate
2. **Hot path** (subsequent requests): The cached delegate is invoked directly

This ensures minimal overhead after the initial setup while maintaining type safety and avoiding code generation.

## Security

- **Opt-in assembly scanning**: Only explicitly provided assemblies are scanned
- **No dynamic code generation**: Uses `MakeGenericMethod` instead of `ILGenerator`
- **Scoped service provider**: Prevents capturing the root container
- **Type filtering**: Only concrete, non-abstract handler classes are registered

## Comparison with MediatR

| Feature | MediatR | VibedMediatr |
|---------|---------|--------------|
| Notifications | ✅ | ❌ (out of scope) |
| Behaviors/Pipelines | ✅ | ❌ (out of scope) |
| Request Pre/Post processing | ✅ | ❌ (out of scope) |
| `IRequest` / `IRequestHandler` | ✅ | ✅ |
| `IMediator.Send()` | ✅ | ✅ |
| Assembly Scanning | ✅ | ✅ (secure, opt-in) |

## Error Handling

VibedMediatr provides clear error messages for common issues:

- **No handler found**: `InvalidOperationException` with details about the missing handler
- **Invalid request type**: `InvalidOperationException` when request doesn't implement required interfaces
- **Null requests**: `ArgumentNullException` for null request parameters

## Examples

See the `VibedMediatr.Example` project for a complete Razor Pages demonstration.

## Contributing

Contributions are welcome! Please submit a pull request.

## License

MIT License - see [LICENSE](LICENSE) file for details.

