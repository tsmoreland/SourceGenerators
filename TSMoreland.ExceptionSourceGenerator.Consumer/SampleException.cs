using System.Diagnostics;
using TSMoreland.ExceptionSourceGenerator.Shared;

namespace TSMoreland.ExceptionSourceGenerator.Consumer;

[DebuggerDisplay("Sample Exception {Message}")]
[AddExceptionConstructors]
public class SampleException
{
}