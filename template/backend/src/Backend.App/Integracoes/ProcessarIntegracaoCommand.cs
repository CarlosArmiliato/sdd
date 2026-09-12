using Cortex.Mediator.Commands;

namespace Backend.App.Integracoes;

public sealed record ProcessarIntegracaoCommand(Guid IntegracaoId) : ICommand;
