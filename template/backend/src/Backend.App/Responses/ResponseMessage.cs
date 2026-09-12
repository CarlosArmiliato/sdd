namespace Backend.App.Responses;

public sealed record ResponseMessage(string Code, string Message, string? Target = null);
