using Backend.Api.Configuration;
using Backend.Api.Contracts;
using Backend.App.Cadastros;
using Backend.Domain.Cadastros;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.BackendUser)]
[Route("api/cadastros")]
public sealed class CadastrosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<CadastroDto>> Listar([FromQuery] CadastroTipo tipo, CancellationToken cancellationToken) =>
        mediator.QueryAsync(new ListarCadastrosQuery(tipo), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CadastroDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        CadastroDto? cadastro = await mediator.QueryAsync(new ObterCadastroQuery(id), cancellationToken);
        return cadastro is null ? NotFound() : Ok(cadastro);
    }

    [HttpPost]
    public async Task<ActionResult<CadastroDto>> Criar(SalvarCadastroRequest request, CancellationToken cancellationToken)
    {
        SalvarCadastroCommand command = new(null, request.Tipo, request.Codigo, request.Nome, request.CodigoExterno);
        CadastroDto cadastro = await mediator.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = cadastro.Id }, cadastro);
    }

    [HttpPut("{id:guid}")]
    public Task<CadastroDto> Atualizar(Guid id, SalvarCadastroRequest request, CancellationToken cancellationToken) =>
        mediator.SendAsync(new SalvarCadastroCommand(id, request.Tipo, request.Codigo, request.Nome, request.CodigoExterno), cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new ExcluirCadastroCommand(id), cancellationToken);
        return NoContent();
    }
}
