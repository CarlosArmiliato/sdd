using System.Net.Http.Json;
using Backend.App.Abstractions.Integrations;
using Backend.Domain.Cadastros;
using Microsoft.Extensions.Options;

namespace Backend.Infra.Gate.Sap;

internal sealed class SapGateway(HttpClient httpClient, IOptions<SapOptions> options) : ISapCadastroSource
{
    public async Task<IReadOnlyCollection<SapCadastro>> BuscarCadastrosAsync(CancellationToken cancellationToken)
    {
        SapCadastroResponse[] response = await httpClient.GetFromJsonAsync<SapCadastroResponse[]>(
            options.Value.CadastrosPath,
            cancellationToken) ?? [];
        return response.Select(item => new SapCadastro(item.Tipo, item.Codigo, item.Nome, item.CodigoExterno)).ToArray();
    }

    private sealed record SapCadastroResponse(CadastroTipo Tipo, string Codigo, string Nome, string CodigoExterno);
}
