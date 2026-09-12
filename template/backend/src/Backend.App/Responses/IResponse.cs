using System.Net;

namespace Backend.App.Responses;

public interface IResponse
{
    public object? Result { get; }
    public bool Success { get; }
    public HttpStatusCode? HttpStatusCode { get; }
    public IReadOnlyCollection<ResponseMessage> RegrasNegocio { get; }
    public IReadOnlyCollection<ResponseMessage> Erros { get; }
    public IReadOnlyCollection<ResponseMessage> Warnings { get; }
    public IReadOnlyCollection<ResponseMessage> Informations { get; }
    public IReadOnlyCollection<ResponseMessage> Debugs { get; }
}
