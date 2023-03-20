using System.Text;
using Microsoft.CodeAnalysis;
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
        context.RegisterSourceOutput(classes, ClassExecute);

        IncrementalValuesProvider<RecordDeclarationSyntax> records = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is RecordDeclarationSyntax,
                transform: static (ctx, _) => (RecordDeclarationSyntax)ctx.Node);
        context.RegisterSourceOutput(records, RecordExecute);
    }

    private static bool TryGetClassAndNamespaceName(TypeDeclarationSyntax typeDeclaration, out string @namespace,
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

    private static void ClassExecute(SourceProductionContext context, ClassDeclarationSyntax classDeclartion)
    {
        if (!TryGetClassAndNamespaceName(classDeclartion, out string @namespace, out string declartionName))
        {
            return;
        }

        string filename = $"{@namespace}.{declartionName}.g.cs";

        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine($$"""
            using System.Diagnostics;
            namespace {{@namespace}}
            {
                [DebuggerDisplay("{GetDebuggerDisplayContent()}")]
                partial class {{declartionName}}
                {
                    private string GetDebuggerDisplayContent()
                    {
                        return "pending...";
                    }
                }
            }
            """);
    
        context.AddSource(filename, sourceBuilder.ToString());
    }

    private static void RecordExecute(SourceProductionContext context, RecordDeclarationSyntax recordDeclaration)
    {
        if (!TryGetClassAndNamespaceName(recordDeclaration, out string @namespace, out string declartionName))
        {
            return;
        }

        string filename = $"{@namespace}.{declartionName}.g.cs";

        StringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine($$"""
            using System.Diagnostics;
            namespace {{@namespace}}
            {
                [DebuggerDisplay("{GetDebuggerDisplayContent()}")]
                partial record {{declartionName}}
                {
                    private string GetDebuggerDisplayContent()
                    {
                        return "pending...";
                    }
                }
            }
            """);
    
        context.AddSource(filename, sourceBuilder.ToString());
    }
}
