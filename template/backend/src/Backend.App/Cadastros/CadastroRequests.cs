using Backend.Domain.Cadastros;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;

namespace Backend.App.Cadastros;

public sealed record CadastroDto(Guid Id, CadastroTipo Tipo, string Codigo, string Nome, string? CodigoExterno)
{
    public static CadastroDto From(Cadastro cadastro) =>
        new(cadastro.Id, cadastro.Tipo, cadastro.Codigo, cadastro.Nome, cadastro.CodigoExterno);
}

public sealed record ListarCadastrosQuery(CadastroTipo Tipo) : IQuery<IReadOnlyCollection<CadastroDto>>;
public sealed record ObterCadastroQuery(Guid Id) : IQuery<CadastroDto?>;
public sealed record SalvarCadastroCommand(
    Guid? Id,
    CadastroTipo Tipo,
    string Codigo,
    string Nome,
    string? CodigoExterno = null) : ICommand<CadastroDto>;
public sealed record ExcluirCadastroCommand(Guid Id) : ICommand;
