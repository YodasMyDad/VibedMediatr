using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VibedMediatr.Example.Requests;

namespace VibedMediatr.Example.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMediator _mediator;

    public string? Reply { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task OnGet()
    {
        Reply = await _mediator.Send(new Ping("Hello from VibedMediatr!"));
    }
}