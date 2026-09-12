using Backend.App.Health;
using Backend.Domain.Health;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(GetHealthStatus healthStatus) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<ServiceHealth>(StatusCodes.Status200OK)]
    public ActionResult<ServiceHealth> Get() => Ok(healthStatus.Handle());
}
