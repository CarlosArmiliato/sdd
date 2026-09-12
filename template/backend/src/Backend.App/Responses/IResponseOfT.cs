namespace Backend.App.Responses;

public interface IResponse<out T> : IResponse
{
    public new T? Result { get; }
}
