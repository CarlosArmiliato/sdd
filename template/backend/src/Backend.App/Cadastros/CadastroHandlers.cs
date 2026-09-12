using Backend.App.Abstractions.Persistence;
using Backend.Domain.Cadastros;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;

namespace Backend.App.Cadastros;

public sealed class ListarCadastrosHandler(ICadastroRepository repository)
    : IQueryHandler<ListarCadastrosQuery, IReadOnlyCollection<CadastroDto>>
{
    public async Task<IReadOnlyCollection<CadastroDto>> Handle(
        ListarCadastrosQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Cadastro> cadastros = await repository.ListarAsync(query.Tipo, cancellationToken);
        return cadastros.Select(CadastroDto.From).ToArray();
    }
}

public sealed class ObterCadastroHandler(ICadastroRepository repository)
    : IQueryHandler<ObterCadastroQuery, CadastroDto?>
{
    public async Task<CadastroDto?> Handle(ObterCadastroQuery query, CancellationToken cancellationToken)
    {
        Cadastro? cadastro = await repository.ObterAsync(query.Id, cancellationToken);
        return cadastro is null ? null : CadastroDto.From(cadastro);
    }
}

public sealed class SalvarCadastroHandler(ICadastroRepository repository)
    : ICommandHandler<SalvarCadastroCommand, CadastroDto>
{
    public async Task<CadastroDto> Handle(SalvarCadastroCommand command, CancellationToken cancellationToken)
    {
        Cadastro cadastro = command.Id is null
            ? new Cadastro(command.Tipo, command.Codigo, command.Nome)
            : await repository.ObterAsync(command.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Cadastro não encontrado.");
        cadastro.Atualizar(command.Codigo, command.Nome, command.CodigoExterno);
        await repository.SalvarAsync(cadastro, cancellationToken);
        return CadastroDto.From(cadastro);
    }
}

public sealed class ExcluirCadastroHandler(ICadastroRepository repository)
    : ICommandHandler<ExcluirCadastroCommand>
{
    public async Task Handle(ExcluirCadastroCommand command, CancellationToken cancellationToken)
    {
        Cadastro cadastro = await repository.ObterAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Cadastro não encontrado.");
        await repository.ExcluirAsync(cadastro, cancellationToken);
    }
}
