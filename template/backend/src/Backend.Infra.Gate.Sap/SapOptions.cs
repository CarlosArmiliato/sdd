namespace Backend.Infra.Gate.Sap;

public sealed class SapOptions
{
    public const string SectionName = "Sap";
    public Uri BaseAddress { get; init; } = null!;
    public string CadastrosPath { get; init; } = "api/cadastros";
}
