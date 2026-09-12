namespace Backend.Domain.Integracoes;

public sealed class Integracao
{
    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Destino { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public IntegracaoStatus Status { get; private set; }
    public int Tentativas { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset DisponivelEm { get; private set; }
    public DateTimeOffset? ProcessamentoIniciadoEm { get; private set; }
    public DateTimeOffset? ProcessadaEm { get; private set; }
    public string? UltimoErro { get; private set; }

    private Integracao() { }

    public Integracao(Guid id, string tipo, string destino, string payload, string correlationId)
    {
        Id = id;
        Tipo = tipo;
        Destino = destino;
        Payload = payload;
        CorrelationId = correlationId;
        Status = IntegracaoStatus.Pendente;
        CriadaEm = DateTimeOffset.UtcNow;
        DisponivelEm = CriadaEm;
    }

    public void Concluir(DateTimeOffset processadaEm)
    {
        Status = IntegracaoStatus.Concluida;
        ProcessadaEm = processadaEm;
        UltimoErro = null;
    }

    public void IniciarProcessamento(DateTimeOffset iniciadoEm)
    {
        if (Status != IntegracaoStatus.Pendente)
        {
            throw new InvalidOperationException("Somente integrações pendentes podem ser processadas.");
        }

        Status = IntegracaoStatus.Processando;
        ProcessamentoIniciadoEm = iniciadoEm;
    }

    public void RegistrarFalha(string erro, DateTimeOffset proximaTentativa)
    {
        Tentativas++;
        Status = Tentativas >= 5 ? IntegracaoStatus.FalhaDefinitiva : IntegracaoStatus.Pendente;
        DisponivelEm = proximaTentativa;
        ProcessamentoIniciadoEm = null;
        UltimoErro = erro;
    }
}
