using FluentValidation;

namespace Backend.App.Cadastros;

public sealed class SalvarCadastroValidator : AbstractValidator<SalvarCadastroCommand>
{
    public SalvarCadastroValidator()
    {
        RuleFor(command => command.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Nome).NotEmpty().MaximumLength(200);
    }
}
