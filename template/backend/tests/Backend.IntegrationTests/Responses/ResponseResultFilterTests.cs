using System.Net;
using Backend.Api.Filters;
using Backend.App.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Backend.IntegrationTests.Responses;

public sealed class ResponseResultFilterTests
{
    [Fact]
    public void SucessoRetornaSomenteResult()
    {
        Response<string> response = new("fazenda");
        ObjectResult objectResult = Execute(response);
        Assert.Equal("fazenda", objectResult.Value);
        Assert.Null(objectResult.StatusCode);
    }

    [Fact]
    public void FalhaRetornaEnvelopeEStatusInformado()
    {
        Response response = new();
        response.AddErro(new("cadastro_nao_encontrado", "Cadastro não encontrado."), HttpStatusCode.NotFound);
        ObjectResult objectResult = Execute(response);
        Assert.Same(response, objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    [Fact]
    public void FalhaSemStatusRetornaBadRequest()
    {
        Response response = new();
        response.AddRegraNegocio(new("regra_invalida", "Regra inválida."));
        ObjectResult objectResult = Execute(response);
        Assert.Same(response, objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    private static ObjectResult Execute(IResponse response)
    {
        ObjectResult objectResult = new(response);
        ActionContext actionContext = new(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        ResultExecutingContext context = new(
            actionContext,
            [],
            objectResult,
            new object());
        new ResponseResultFilter().OnResultExecuting(context);
        return objectResult;
    }
}
