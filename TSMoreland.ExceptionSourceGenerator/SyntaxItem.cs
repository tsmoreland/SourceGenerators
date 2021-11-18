namespace TSMoreland.ExceptionSourceGenerator;

internal sealed class SyntaxItem
{
    public string Namespace { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;

    public string Fullname => $"{Namespace}.{ClassName}";
}