﻿//
// Copyright © 2021 Terry Moreland
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TSMoreland.ExceptionSourceGenerator;

[Generator]
public sealed class ExceptionGenerator : ISourceGenerator
{
    private const string _generatorAttributeName = "TSMoreland.ExceptionSourceGenerator.Shared.ExceptionGeneratorAttribute";


    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
        {
            context.AddSource("GeneratorLogs", SourceText.From($@"/*{ context.SyntaxContextReceiver?.GetType().FullName ?? "Unknown"}*/", Encoding.UTF8));
            return;
        }

        receiver.Log.Add($"Found {receiver.Items.Count()} items.");

        foreach (var item in receiver.Items)
        {
            context.AddSource($"{item.Fullname}.g.cs", SourceText.From(item.Generate(), Encoding.UTF8));
        }

        context.AddSource("GeneratorLogs", SourceText.From($@"/*{ Environment.NewLine + string.Join(Environment.NewLine, receiver.Log) + Environment.NewLine}*/", Encoding.UTF8));

    }


    private sealed class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        private readonly List<ExceptionItem> _items = new ();
        public List<string> Log { get; } = new();

        public IEnumerable<ExceptionItem> Items => _items.AsEnumerable();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            try
            {
                SafeVisitSyntaxNode(context);
            }
            catch (Exception)
            {
                // ...
            }
        }

        private void SafeVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax)
            {
                return;
            }

            var testClass = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
            var @namespace = testClass.ContainingNamespace.ToString();

            if (IsGlobalNamespace(@namespace))
            {
                Log.Add($"Ignoring global namespace {@namespace}");
                return;
            }

            var className = testClass.Name;

            Log.Add("Namespace: " + @namespace);
            Log.Add("Class: " + className);

            // TODO: determine if class is sealed, if it is we should track that so anything that might be protcted uses private instaed

            // Intent is to eventual handle others where each item defines a property 
            var allAttributes = testClass.GetAttributes();
            Log.AddRange(allAttributes.Select(attribute => $"Found {attribute.AttributeClass!.ContainingNamespace}.{attribute.AttributeClass!.Name}, looking for {_generatorAttributeName}"));
            var attributes = allAttributes.Where(a => $"{a.AttributeClass!.ContainingNamespace}.{a.AttributeClass!.Name}" == _generatorAttributeName).ToArray();

            if (attributes.Any())
            {
                var item = new ExceptionItem(@namespace, className);
                Dictionary<string, PropertyItem> properties = new();
                Log.Add($"Adding {@namespace}.{className} total: {attributes.Length}");
                Dictionary<string, (string?, string?, bool)> propertiesByName = new();
                for (int i = 0; i < attributes.Length; i++)
                {
                    var attribute = attributes[i];
                    string? name = null;
                    string? type = null;
                    string? description = null;
                    string? defaultValue = null;
                    var isReadonly = true;
                    Log.Add($"{@namespace}.{className} position {i} of {attributes.Length}"); 
                    Log.Add($"{@namespace}.{className} has {attribute.ConstructorArguments.Length} constructor arguments");
                    Log.Add($"{@namespace}.{className} has {attribute.NamedArguments.Length} named arguments");

                    Log.Add($"ctor args: {attribute.ConstructorArguments.Length}");
                    foreach (var argument in attribute.ConstructorArguments)
                    {
                        Log.Add(argument.ToString());
                        Log.Add(argument.Value?.ToString() ?? string.Empty);
                        Log.Add(argument.Values.Select(o => o.ToString()).Aggregate((a,b) => $"{a}, {b}"));
                    }

                    Log.Add($"positional args: {attribute.NamedArguments.Length}");
                    foreach (var argument in attribute.NamedArguments)
                    {
                        switch (argument.Key)
                        {
                            case "IsReadOnly":
                                isReadonly = argument.Value.Value is true;
                                break;
                            case "PropertyName":
                                name = argument.Value.Value?.ToString() ?? string.Empty;
                                break;
                            case "PropertyType":
                                type = argument.Value.Value?.ToString() ?? string.Empty;
                                break;
                            case "PropertyDescription":
                                description = argument.Value.Value?.ToString() ?? string.Empty;
                                break;
                            case "DefaultValue":
                                defaultValue = argument.Value.Value?.ToString();
                                break;
                        }

                        Log.Add($"position arg: {argument.Key}");
                        Log.Add($"position arg value: {argument.Value.Value?.ToString() ?? "unknown"}");
                    }

                    if (name is { Length: > 0 } && type is { Length: > 0 })
                    {
                        properties[name] = new PropertyItem(name, type, isReadonly, description ?? string.Empty, defaultValue);
                    }

                    Log.Add($"attribute complete");
                }
                _items.Add(item with {Properties = properties.Values.ToList()});
            }
            else
            {
                Log.Add($"Attribute not found");
            }
        }


        private static bool IsGlobalNamespace(string @namepsace)
        {
            // bit of a simple check that could be improved
            return namepsace.Contains("<");
        }

    }
}