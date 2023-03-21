using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSMoreland.SourceGenerators.DebugDisplay.Generator;

[Generator]
public sealed class DebugDisplayGenerator : IIncrementalGenerator // preferred over ISourceGenerator for speed - only builds when it needs to
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // get all class declaration syntax nodes
        IncrementalValuesProvider<ClassDeclarationSyntax> classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetWithAttribute(node),
                transform: static (ctx, _) => GetSemanticTarget(ctx)!) // GetSemanticTarget can return null but it's caught by the where clause
            .Where(static target => target is not null);
        context.RegisterSourceOutput(classes, static (ctx, source) =>  Execute(ctx, source));
        context.RegisterPostInitializationOutput(static ctx => PostInitializationOutput(ctx));

        IncrementalValuesProvider<RecordDeclarationSyntax> records = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is RecordDeclarationSyntax,
                transform: static (ctx, _) => (RecordDeclarationSyntax)ctx.Node);
        context.RegisterSourceOutput(records, static (ctx, declaration) => Execute(ctx, declaration));
    }

    private static bool IsSyntaxTargetWithAttribute(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }
    private static ClassDeclarationSyntax? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        INamedTypeSymbol? attributeSymbol = context.SemanticModel.Compilation
            .GetTypeByMetadataName("TSMoreland.SourceGenerators.DebugDisplay.Generator.GenerateDebugDisplayAttribute");

        bool hasGenerateDebugDisplayAttribute = classSymbol is not null && classSymbol.GetAttributes()
            .Any(symbol => symbol.AttributeClass?.Equals(attributeSymbol) is true);

        return hasGenerateDebugDisplayAttribute
            ? classDeclaration
            : null;
    }

    private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("TSMoreland.SourceGenerators.DebugDisplay.Generator.GenerateDebugDisplayAttribute.g.cs", """
            namespace TSMoreland.SourceGenerators.DebugDisplay.Generator
            {
                internal sealed class GenerateDebugDisplayAttribute : System.Attribute
                {
                }
            }
            """);
    }

    private static bool TryGetClassAndNamespaceName(BaseTypeDeclarationSyntax typeDeclaration, out string @namespace,
        out string declarationName)
    {
        if (typeDeclaration.Parent is not BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            @namespace = declarationName = string.Empty;
            return false;
        }

        @namespace = namespaceDeclarationSyntax.Name.ToString();
        declarationName = typeDeclaration.Identifier.Text;
        return true;
    }

    private static void Execute(SourceProductionContext context,
        TypeDeclarationSyntax declaration)
    {
        List<ParameterSyntax> recordParameters = declaration is RecordDeclarationSyntax { ParameterList: not null } recordDeclaration
            ? recordDeclaration.ParameterList.Parameters.ToList()
            : new List<ParameterSyntax>();

        List<PropertyDeclarationSyntax> properties = declaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(SyntaxKind.PublicKeyword))
            .ToList();
        if (!properties.Any() && !recordParameters.Any())
        {
            return;
        }

        if (!TryGetClassAndNamespaceName(declaration, out string @namespace, out string declarationName))
        {
            return;
        }
        string declarationType = declaration switch
        {
            ClassDeclarationSyntax _ => "class",
            RecordDeclarationSyntax => "record",
            _ => string.Empty,
        };

        if (declarationType is not { Length: > 0 })
        {
            return;
        }

        string filename = $"{@namespace}.{declarationName}.g.cs";

        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine($$"""
            using System.Diagnostics;
            namespace {{@namespace}}
            {
            """);


        sourceBuilder.Append("    [DebuggerDisplay(\"");
        foreach (string parameterName in recordParameters.Select(parameter => parameter.Identifier.Text))
        {
            sourceBuilder.Append($"{parameterName}: {{{parameterName}Truncated}} ");
        }
        foreach (string propertyName in properties.Select(property => property.Identifier.Text))
        {
            sourceBuilder.Append($"{propertyName}: {{{propertyName}Truncated}} ");
        }
        sourceBuilder.AppendLine("\")]");

        sourceBuilder.AppendLine($$"""
                partial {{declarationType}} {{declarationName}}
                {
            """);

        foreach (ParameterSyntax parameter in recordParameters)
        {
            sourceBuilder.Append(GenerateTruncatedProperty(parameter.Identifier.Text));
        }

        foreach (string propertyName in properties.Select(property => property.Identifier.Text))
        {
            sourceBuilder.Append(GenerateTruncatedProperty(propertyName));
        }

        sourceBuilder.AppendLine("""
                }
            }
            """);

        context.AddSource(filename, sourceBuilder.ToString());
    }

    private static string GenerateTruncatedProperty(string propertyName)
    {
        return $$"""

                private string {{propertyName}}Truncated 
                {
                    get
                    {
                        string content = {{propertyName}} != default!
                            ? {{propertyName}}.ToString()
                            : string.Empty;

                        if (content is not { Length: > 10 })
                        {
                            return content;
                        }
                        return $"{content[..10]}...";
                    }
                }

        """;
    }
}
