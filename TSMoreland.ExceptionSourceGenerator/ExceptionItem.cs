namespace TSMoreland.ExceptionSourceGenerator;

internal readonly record struct ExceptionItem(
    string Namespace, 
    string ClassName, 
    IReadOnlyList<PropertyItem> Properties)
{

    public ExceptionItem(string @namespace, string classname)
        : this(@namespace, classname, new List<PropertyItem>())
    {

    }

    public string Fullname => $"{Namespace}.{ClassName}";
}