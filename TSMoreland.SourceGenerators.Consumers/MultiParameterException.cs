using TSMoreland.SourceGenerators.Exceptions;

namespace TSMoreland.SourceGenerators.Consumers;

[ExceptionGenerator(IsReadOnly = true, PropertyName = "Alpha", PropertyType = "int")]
[ExceptionGenerator(IsReadOnly = true, PropertyName = "Bravo", PropertyType = "System.Guid")]
public partial class MultiParameterException
{
}
