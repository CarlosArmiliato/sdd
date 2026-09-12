using Backend.Api.Configuration;
using Backend.App.BackgroundJobs;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.BackendUser)]
[Route("api/jobs")]
public sealed class JobsController(IMediator mediator) : ControllerBase
{
    [HttpPost("sincronizacao-sap")]
    [ProducesResponseType<AgendamentoDto>(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<AgendamentoDto>> AgendarSincronizacaoSap(CancellationToken cancellationToken)
    {
        string jobId = await mediator.SendAsync(new AgendarSincronizacaoSapCommand(), cancellationToken);
        return Accepted(new AgendamentoDto(jobId));
    }
}

public sealed record AgendamentoDto(string JobId);
