namespace TSMoreland.SourceGenerators.Exceptions;

/// <summary>
/// Marks a class for application of source generator, if present the constructors
/// and properties are included.
/// </summary>
/// <remarks>
/// can be used multiple times to handle multiple properties
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ExceptionGeneratorAttribute : Attribute
{

    /// <summary>
    /// Constructs a new instance of the <see cref="ExceptionGeneratorAttribute"/> class.
    /// </summary>
    public ExceptionGeneratorAttribute()
    {
    }

    /// <value>
    /// Type for a property to be included in the exception
    /// </value>
    public string? PropertyType { get; set; }

    /// <value>
    /// Name for a property to be included in the exception
    /// </value>
    public string? PropertyName { get; set; }

    /// <summary>
    /// description to be used in comments
    /// </summary>
    public string? PropertyDescription { get; set; }

    /// <value>
    /// </value>    
    public string? PropertyDefaultValue { get; set; }

    /// <value>
    /// defaults to <see langword="true"/>,  property be readonly
    /// </value>
    public bool IsReadOnly { get; set; } = true;
}
