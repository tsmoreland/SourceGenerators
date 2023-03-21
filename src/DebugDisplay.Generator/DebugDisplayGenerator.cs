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
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node);
        context.RegisterSourceOutput(classes, TypeDeclarationExecute);

        IncrementalValuesProvider<RecordDeclarationSyntax> records = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is RecordDeclarationSyntax,
                transform: static (ctx, _) => (RecordDeclarationSyntax)ctx.Node);
        context.RegisterSourceOutput(records, TypeDeclarationExecute);
    }

    private static bool TryGetClassAndNamespaceName(BaseTypeDeclarationSyntax typeDeclaration, out string @namespace,
        out string declartionName)
    {
        if (typeDeclaration.Parent is not BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            @namespace = declartionName = string.Empty;
            return false;
        }

        @namespace = namespaceDeclarationSyntax.Name.ToString();
        declartionName = typeDeclaration.Identifier.Text;
        return true;
    }

    private static void TypeDeclarationExecute(SourceProductionContext context,
        TypeDeclarationSyntax declaration)
    {
        List<ParameterSyntax> recordParameters = declaration is RecordDeclarationSyntax { ParameterList: not null }  recordDeclaration
            ? recordDeclaration.ParameterList.Parameters
                .Where(p => p.Modifiers.Any(SyntaxKind.PublicKeyword))
                .ToList() 
            : new List<ParameterSyntax>();

        List<PropertyDeclarationSyntax> properties = declaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(SyntaxKind.PublicKeyword))
            .ToList();
        if (!properties.Any() && !recordParameters.Any())
        {
            return;
        }

        if (!TryGetClassAndNamespaceName(declaration, out string @namespace, out string declartionName))
        {
            return;
        }
        string declartionType = declaration switch
        {
            ClassDeclarationSyntax _ => "class",
            RecordDeclarationSyntax => "record",
            _ => string.Empty,
        };

        if (declartionType is not { Length: > 0 })
        {
            return;
        }

        string filename = $"{@namespace}.{declartionName}.g.cs";

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
        sourceBuilder.Append("\")]");

        sourceBuilder.AppendLine($$"""
                partial {{declartionType}} {{declartionName}}
                {
            """);

        foreach (ParameterSyntax parameter in recordParameters)
        {
            sourceBuilder.Append(GenerateTruncatedProperty(parameter.Identifier.Text));
        }
        /*
        foreach (string parameterName in recordParameters.Select(parameter => parameter.Identifier.Text))
        {
            sourceBuilder.Append(GenerateTruncatedProperty(parameterName));
        }
        */

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
