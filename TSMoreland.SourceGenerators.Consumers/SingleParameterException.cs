using TSMoreland.SourceGenerators.Exceptions;

namespace TSMoreland.SourceGenerators.Consumers;

[ExceptionGenerator(IsReadOnly = true, PropertyName = "Single", PropertyType = "int", PropertyDescription = "Sample comment")]
public partial class SingleParameterException
{
}
