//
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

using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TSMoreland.ExceptionSourceGenerator.Shared;

namespace TSMoreland.ExceptionSourceGenerator;

[Generator]
public sealed class ExceptionGenerator : ISourceGenerator
{
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
            return;
        }

        foreach (var item in receiver.Items)
        {
            var builder = new StringBuilder();
            builder.Append($@"
#nullable enable
namespace {item.Namespace}
{{
    [System.SerializableAttribute]
    [System.CodeDom.Compiler.GeneratedCodeAttribute(""{nameof(ExceptionGenerator)}"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]

    public partial class {item.ClassName} : System.Exception
    {{
        /// <summary>
        /// Initializes a new instance of the <see cref=""{item.ClassName}""/> class.
        /// </summary>
        public {item.ClassName}()
            : this(null, null)
        {{
            
        }}

        /// <summary>
        /// Initializes a new instance of the <see cref=""{item.ClassName}""/> class with a specified error message.
        /// </summary>
        /// <param name=""message"">The error message that explains the reason for the exception.</param>
        public {item.ClassName}(string? message)
            : this(message, null)
        {{
            
        }}

        /// <summary>
        /// Initializes a new instance of the <see cref=""{item.ClassName}""/> class with a specified error message and
        /// a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name=""message"">The error message that explains the reason for the exception.</param>
        /// <param name=""innerException"">
        /// The exception that is the cause of the current exception, or a <see langword=""null""/> reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public {item.ClassName}(string? message, System.Exception? innerException)
            : base(message, innerException)
        {{
            
        }}

        protected {item.ClassName}(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {{
            
        }}
    }}
}}
");
            context.AddSource(item.ClassName, SourceText.From(builder.ToString(), Encoding.UTF8));
        }

        context.AddSource("GeneratorLogs", SourceText.From($@"/*{ Environment.NewLine + string.Join(Environment.NewLine, receiver.Log) + Environment.NewLine}*/", Encoding.UTF8));

    }

    public sealed class SyntaxItem
    {
        public string FullClassName => $"{Namespace}.{FullClassName}";
        public string Namespace { get; init; } = string.Empty;
        public string ClassName { get; init; } = string.Empty;
    }

    /// <inheritdoc/>
    private sealed class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        private readonly List<SyntaxItem> _items = new ();
        public List<string> Log { get; } = new();

        public IEnumerable<SyntaxItem> Items => _items.AsEnumerable();

        /// <inheritdoc/>
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
                return;
            }

            var className = testClass.Name;
            var fullClassName = $"{@namespace}.{className}";

            Log.Add("Namespace: " + @namespace);
            Log.Add("Class: " + className);

            // TODO: determine if class is sealed, if it is we should track that so anything that might be protcted uses private instaed


            // Intent is to eventual handle others where each item defines a property 
            var attributes = testClass.GetAttributes()
                .Where(a => $"{a.AttributeClass!.ContainingNamespace}.{a.AttributeClass!.Name}" == typeof(AddExceptionConstructorsAttribute).FullName);
            if (attributes.Any())
            {
                _items.Add(new SyntaxItem { Namespace = @namespace, ClassName = className });
            }
        }

        private static bool IsGlobalNamespace(string @namepsace)
        {
            // bit of a simple check that could be improved
            return namepsace.Contains("<");
        }
    }
}