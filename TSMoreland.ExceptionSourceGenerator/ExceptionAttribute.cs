namespace TSMoreland.ExceptionSourceGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExceptionAttribute : Attribute
{

    /// <summary>
    /// Constructs a new instance of the <see cref="ExceptionAttribute"/> class.
    /// </summary>
    /// <param name="properties">optional properties to include in the generated class</param>
    public ExceptionAttribute(params (Type Type, string Name)[] properties)
    {
        Properties = properties;
    }

    /// <summary>
    /// Exception Properties
    /// </summary>
    public (Type Type, string Name)[] Properties { get; }
}