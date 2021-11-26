namespace TSMoreland.ExceptionSourceGenerator;

internal readonly record struct PropertyItem(string Name, string Type, bool IsReadOnly, string? DefaultValue = null)
{
    public bool HasDefaultValue => DefaultValue is { Length: > 0 };

    /// <inheritdoc/>
    public override string ToString()
    {
        return HasDefaultValue
            ? $"{Type} {Name} = {DefaultValue}"
            : $"{Type} {Name}";
    }
}