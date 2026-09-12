using Backend.Domain.Cadastros;

namespace Backend.UnitTests.Cadastros;

public sealed class CadastroTests
{
    [Fact]
    public void CriarEAtualizarNormalizaDados()
    {
        Cadastro cadastro = new(CadastroTipo.Fazenda, " F01 ", " Fazenda Sul ");
        cadastro.Atualizar(" F02 ", " Fazenda Norte ", "SAP-2");
        Assert.Equal(CadastroTipo.Fazenda, cadastro.Tipo);
        Assert.Equal("F02", cadastro.Codigo);
        Assert.Equal("Fazenda Norte", cadastro.Nome);
        Assert.Equal("SAP-2", cadastro.CodigoExterno);
        Assert.NotEqual(Guid.Empty, cadastro.Id);
    }
}
