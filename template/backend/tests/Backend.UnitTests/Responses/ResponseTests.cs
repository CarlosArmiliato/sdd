using System.Net;
using Backend.App.Responses;

namespace Backend.UnitTests.Responses;

public sealed class ResponseTests
{
    [Fact]
    public void AppendRetornaResultadoEAgregaMensagens()
    {
        Response<string> responseA = new("fazenda");
        responseA.AddWarning(new("cadastro_warning", "Cadastro possui uma advertência."));
        Response responseB = new();
        string? result = responseB.Append(responseA);
        Assert.Equal("fazenda", result);
        Assert.Equal("fazenda", ((IResponse)responseA).Result);
        Assert.Equal("fazenda", ((Response)responseA).Result);
        Assert.True(responseB.Success);
        Assert.Single(responseB.Warnings);
    }

    [Fact]
    public void AppendPreservaPrimeiroStatusDeFalha()
    {
        Response primeira = CreateFailure(HttpStatusCode.NotFound);
        Response segunda = CreateFailure(HttpStatusCode.Conflict);
        Response aggregate = new();
        aggregate.Append(primeira);
        aggregate.Append(segunda);
        Assert.False(aggregate.Success);
        Assert.Equal(HttpStatusCode.NotFound, aggregate.HttpStatusCode);
        Assert.Equal(2, aggregate.Erros.Count);
    }

    [Fact]
    public void FalhaSemStatusPermaneceSemStatusParaFallbackDaApi()
    {
        Response response = new();
        response.AddRegraNegocio(new("checklist_incompleto", "Checklist incompleto."));
        Assert.False(response.Success);
        Assert.Null(response.HttpStatusCode);
    }

    [Fact]
    public void FalhaRejeitaStatusDeSucesso()
    {
        Response response = new();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            response.AddErro(new("erro", "Erro."), HttpStatusCode.OK));
    }

    private static Response CreateFailure(HttpStatusCode httpStatusCode)
    {
        Response response = new();
        response.AddErro(new("erro", "Erro."), httpStatusCode);
        return response;
    }
}
