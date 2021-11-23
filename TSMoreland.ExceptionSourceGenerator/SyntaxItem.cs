namespace TSMoreland.ExceptionSourceGenerator;

internal readonly record struct SyntaxItem(
    string Namespace, 
    string ClassName, 
    string? PropertyType,
    string? PropertyName,
    string? PropertyDefaultValue,
    bool IsReadOnly)
{

    public SyntaxItem(string @namespace, string classname)
        : this(@namespace, classname, null, null, null, true)
    {

    }

    public string Fullname => $"{Namespace}.{ClassName}";
}
