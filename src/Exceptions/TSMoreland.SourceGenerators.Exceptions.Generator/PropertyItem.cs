namespace TSMoreland.SourceGenerators.Exceptions.Generator;

internal readonly record struct PropertyItem(string Name, string Type, bool IsReadOnly, string Description, string? DefaultValue = null)
{
    public bool HasDefaultValue => DefaultValue is { Length: > 0 };

    public string CamelCaseTypeAndName => HasDefaultValue
        ? $"{Type} {ToCamelCase(Name)} = {DefaultValue}"
        : $"{Type} {ToCamelCase(Name)}";

    public string CamelCaseName => ToCamelCase(Name);

    private static string ToCamelCase(string source)
    {
        if (source is not { Length: > 0 })
        {
            return source;
        }

        return source.Length > 1
            ? source.Substring(0, 1).ToLowerInvariant() + source.Substring(1, source.Length - 1)
            : source.Substring(0, 1).ToLowerInvariant();

    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return HasDefaultValue
            ? $"{Type} {Name} = {DefaultValue}"
            : $"{Type} {Name}";
    }
}
