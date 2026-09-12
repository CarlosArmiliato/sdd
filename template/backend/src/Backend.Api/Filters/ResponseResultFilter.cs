using System.Net;
using Backend.App.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Api.Filters;

public sealed class ResponseResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult { Value: IResponse response } objectResult)
        {
            return;
        }
        if (response.Success)
        {
            objectResult.Value = response.Result;
            return;
        }
        objectResult.StatusCode = (int)(response.HttpStatusCode ?? HttpStatusCode.BadRequest);
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
