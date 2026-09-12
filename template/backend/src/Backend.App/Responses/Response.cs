using System.Net;

namespace Backend.App.Responses;

public class Response : IResponse
{
    private readonly List<ResponseMessage> _regrasNegocio = [];
    private readonly List<ResponseMessage> _erros = [];
    private readonly List<ResponseMessage> _warnings = [];
    private readonly List<ResponseMessage> _informations = [];
    private readonly List<ResponseMessage> _debugs = [];

    public object? Result => ResultValue;
    public bool Success { get; private set; } = true;
    public HttpStatusCode? HttpStatusCode { get; private set; }
    public IReadOnlyCollection<ResponseMessage> RegrasNegocio => _regrasNegocio;
    public IReadOnlyCollection<ResponseMessage> Erros => _erros;
    public IReadOnlyCollection<ResponseMessage> Warnings => _warnings;
    public IReadOnlyCollection<ResponseMessage> Informations => _informations;
    public IReadOnlyCollection<ResponseMessage> Debugs => _debugs;
    protected object? ResultValue { get; set; }

    public T? Append<T>(IResponse<T> response)
    {
        Append((IResponse)response);
        return response.Result;
    }

    public void Append(IResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (ReferenceEquals(this, response))
        {
            throw new InvalidOperationException("Uma response não pode ser adicionada a ela mesma.");
        }
        _regrasNegocio.AddRange(response.RegrasNegocio);
        _erros.AddRange(response.Erros);
        _warnings.AddRange(response.Warnings);
        _informations.AddRange(response.Informations);
        _debugs.AddRange(response.Debugs);
        if (!response.Success)
        {
            MarkAsFailure(response.HttpStatusCode);
        }
    }

    public void AddRegraNegocio(ResponseMessage message, HttpStatusCode? httpStatusCode = null)
    {
        _regrasNegocio.Add(message);
        MarkAsFailure(httpStatusCode);
    }

    public void AddErro(ResponseMessage message, HttpStatusCode? httpStatusCode = null)
    {
        _erros.Add(message);
        MarkAsFailure(httpStatusCode);
    }

    public void AddWarning(ResponseMessage message) => _warnings.Add(message);

    public void AddInformation(ResponseMessage message) => _informations.Add(message);

    public void AddDebug(ResponseMessage message) => _debugs.Add(message);

    private void MarkAsFailure(HttpStatusCode? httpStatusCode)
    {
        if (httpStatusCode is not null && (int)httpStatusCode < 400)
        {
            throw new ArgumentOutOfRangeException(
                nameof(httpStatusCode),
                httpStatusCode,
                "Uma response com falha deve usar um status HTTP igual ou superior a 400.");
        }
        Success = false;
        HttpStatusCode ??= httpStatusCode;
    }
}
