using TSMoreland.ExceptionSourceGenerator.Shared;

namespace TSMoreland.ExceptionSourceGenerator.Consumer;

[ExceptionGenerator(IsReadOnly = true, PropertyName = "Alpha", PropertyType = "int")]
[ExceptionGenerator(IsReadOnly = true, PropertyName = "Bravo", PropertyType = "System.Guid")]
public partial class MultiParameterException
{
}
