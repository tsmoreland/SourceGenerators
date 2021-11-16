namespace TSMoreland.ExceptionSourceGenerator.Shared;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AddExceptionConstructorsAttribute : Attribute
{

    /// <summary>
    /// Constructs a new instance of the <see cref="AddExceptionConstructorsAttribute"/> class.
    /// </summary>
    public AddExceptionConstructorsAttribute()
    {
    }

}