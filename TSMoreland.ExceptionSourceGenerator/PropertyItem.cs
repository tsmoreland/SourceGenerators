namespace TSMoreland.ExceptionSourceGenerator;

internal readonly record struct PropertyItem(string Name, string Type, bool IsReadOnly, string? DefaultValue = null);