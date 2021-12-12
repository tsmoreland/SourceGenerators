using TSMoreland.ExceptionSourceGenerator.Shared;

namespace TSMoreland.ExceptionSourceGenerator.Consumer;

[ExceptionGenerator(IsReadOnly = true, PropertyName = "Single", PropertyType = "int", PropertyDescription = "Sample comment")]
public partial class SingleParameterException
{
}
