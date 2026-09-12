using Backend.Api.Configuration;
using Backend.Api.Contracts;
using Backend.App.Tickets;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.BackendUser)]
[Route("api/tickets")]
public sealed class TicketsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        TicketDto? ticket = await mediator.QueryAsync(new ObterTicketQuery(id), cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPut("{id:guid}/checklist")]
    public async Task<IActionResult> ResponderChecklist(
        Guid id,
        ResponderChecklistRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new ResponderChecklistCommand(id, request.Respostas, request.CorrelationId), cancellationToken);
        return NoContent();
    }
}
