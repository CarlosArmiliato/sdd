namespace Backend.App.Responses;

public sealed class Response<T> : Response, IResponse<T>
{
    public new T? Result
    {
        get => (T?)ResultValue;
        private set => ResultValue = value;
    }

    public Response() { }

    public Response(T result) => Result = result;

    public void SetResult(T? result) => Result = result;
}
