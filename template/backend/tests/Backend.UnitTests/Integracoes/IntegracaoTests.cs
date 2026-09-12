using Backend.Domain.Integracoes;

namespace Backend.UnitTests.Integracoes;

public sealed class IntegracaoTests
{
    [Fact]
    public void ConcluirRegistraResultado()
    {
        DateTimeOffset agora = DateTimeOffset.UtcNow;
        Integracao integracao = Criar();
        integracao.IniciarProcessamento(agora);
        integracao.Concluir(agora.AddMinutes(1));
        Assert.Equal(IntegracaoStatus.Concluida, integracao.Status);
        Assert.Equal(agora.AddMinutes(1), integracao.ProcessadaEm);
        Assert.Null(integracao.UltimoErro);
    }

    [Fact]
    public void QuintaFalhaEncerraTentativas()
    {
        Integracao integracao = Criar();
        for (int tentativa = 0; tentativa < 5; tentativa++)
        {
            integracao.IniciarProcessamento(DateTimeOffset.UtcNow);
            integracao.RegistrarFalha("TimeoutException", DateTimeOffset.UtcNow.AddMinutes(1));
        }
        Assert.Equal(IntegracaoStatus.FalhaDefinitiva, integracao.Status);
        Assert.Equal(5, integracao.Tentativas);
        Assert.Equal("TimeoutException", integracao.UltimoErro);
        Assert.Null(integracao.ProcessamentoIniciadoEm);
    }

    [Fact]
    public void NaoIniciaQuandoNaoEstaPendente()
    {
        Integracao integracao = Criar();
        integracao.IniciarProcessamento(DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => integracao.IniciarProcessamento(DateTimeOffset.UtcNow));
    }

    private static Integracao Criar() => new(Guid.NewGuid(), "Resultado.v1", "EmpresaA", "{}", "corr-1");
}
