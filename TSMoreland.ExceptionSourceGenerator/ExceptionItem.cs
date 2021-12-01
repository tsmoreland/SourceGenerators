using System.Reflection;
using System.Text;

namespace TSMoreland.ExceptionSourceGenerator;

internal readonly record struct ExceptionItem(
    string Namespace, 
    string ClassName, 
    IReadOnlyList<PropertyItem> Properties)
{

    public ExceptionItem(string @namespace, string classname)
        : this(@namespace, classname, new List<PropertyItem>())
    {

    }

    public string Fullname => $"{Namespace}.{ClassName}";

    public string Generate()
    {
        StringBuilder builder = new();

        builder.Append($@"
#nullable enable
namespace {Namespace}
{{
    [System.SerializableAttribute]
    [System.CodeDom.Compiler.GeneratedCodeAttribute(""{nameof(ExceptionGenerator)}"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]

    public partial class {ClassName} : System.Exception
    {{
");
        AddConstructor(builder);
        AddMessageConstructor(builder, this);
        AddInnerExceptionConstructor(builder, this);
        AddSerializationConstructor(builder);
        AddProperties(builder);
        builder.Append($@"
    }}
}}");
        return builder.ToString();
    }

    private void AddConstructor(StringBuilder builder)
    {
        if (Properties.Any())
        {
            AddConstructorWithProperties(builder);
        }
        else
        {
            AddConstructorWithoutProperties(builder);
        }
    }

    private void AddConstructorWithProperties(StringBuilder builder)
    {
        builder.Append($@"
        /// <summary>
        /// Initializes a new instance of the <see cref=""{ClassName}""/> class.
        /// </summary>");

        foreach (var property in Properties)
        {
            builder.Append($@"
    /// <param name=""{property.Name}"">{property.Description}</param>");
        }

        builder.Append($@"
        public {ClassName}(/*");
            StringBuilder argumentBuilder = new();
            foreach (var property in Properties.Select(p => p.CamelCaseTypeAndName))
            {
                argumentBuilder.Append(property + ", ");
            }

            var arguments = argumentBuilder.ToString();
            builder.Append(arguments.Substring(0, arguments.Length - 2));
            builder.Append(@"*/)
            : this(/*");

            StringBuilder constructorArgs = new();
            foreach (var property in Properties)
            {
                constructorArgs.Append(property.CamelCaseName + ", ");
            }
            builder.Append(constructorArgs);
            builder.Append(@"*/ null, null)
        {
        }");
    }
    private void AddConstructorWithoutProperties(StringBuilder builder)
    {
        builder.Append($@"
        /// <summary>
        /// Initializes a new instance of the <see cref=""{ClassName}""/> class.
        /// </summary>
        public {ClassName}()
            : this(null, null)
        {{
            
        }}");
    }

    private void AddMessageConstructor(StringBuilder builder)
    {
        if (Properties.Any())
        {
            AddMessageConstructorWithProperties(builder);
        }
        else
        {
            AddMessageConstructorWithoutProperties(builder);
        }
    }

    private void AddMessageConstructorWithProperties(StringBuilder builder)
    {
            builder.Append($@"
        /// <summary>
        /// Initializes a new instance of the <see cref=""{ClassName}""/> class with a specified error message.
        /// </summary>");

        foreach (var property in Properties)
        {
            builder.Append($@"
    /// <param name=""{property.Name}"">{property.Description}</param>");
        }

            builder.Append($@"
        /// <param name=""message"">The error message that explains the reason for the exception.</param>
        public {ClassName}(/*");

            StringBuilder argumentBuilder = new();
            foreach (var property in Properties.Select(p => p.CamelCaseTypeAndName))
            {
                argumentBuilder.Append(property + ", ");
            }

            builder.Append(argumentBuilder);
            builder.Append(@"*/ string? message)
            : this(/*");

            StringBuilder constructorArgs = new();
            foreach (var property in Properties)
            {
                constructorArgs.Append(property.CamelCaseName + ", ");
            }
            builder.Append(constructorArgs);
            builder.Append(@"*/ message, null)
        {
");
    }
    private void AddMessageConstructorWithoutProperties(StringBuilder builder)
    {
            builder.Append($@"
        /// <summary>
        /// Initializes a new instance of the <see cref=""{ClassName}""/> class with a specified error message.
        /// </summary>
        /// <param name=""message"">The error message that explains the reason for the exception.</param>
        public {ClassName}(string? message)
            : this(message, null)
        {{
            
        }}
");
    }

    private static void AddInnerExceptionConstructor(StringBuilder builder, ExceptionItem item)
    {
        if (item.Properties.Any())
        {
            builder.Append($@"
        /// <summary>
        /// Initializes a new instance of the <see cref=""{item.ClassName}""/> class with a specified error message and
        /// a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name=""message"">The error message that explains the reason for the exception.</param>
        /// <param name=""innerException"">
        /// The exception that is the cause of the current exception, or a <see langword=""null""/> reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public {item.ClassName}(/*");

            StringBuilder argumentBuilder = new();
            foreach (var property in item.Properties.Select(p => p.CamelCaseTypeAndName))
            {
                argumentBuilder.Append(property + ", ");
            }

            builder.Append(argumentBuilder);

            builder.Append($@"*/ string? message, System.Exception? innerException)
            : base(/* ");

            StringBuilder constructorArgs = new();
            foreach (var property in item.Properties)
            {
                constructorArgs.Append(property.CamelCaseName + ", ");
            }
            builder.Append(constructorArgs);

            builder.Append($@"*/message, innerException)
        {{
        /*
");
            foreach (var property in item.Properties)
            {
                builder.AppendLine($@"            {property.Name} = {property.CamelCaseName};");
            }

            builder.Append(@"
        */
        }
");
        }
        else
        {
            builder.Append($@"
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
");
        }
    }
    private void AddSerializationConstructor(StringBuilder builder)
    {
        builder.Append($@"
        protected {ClassName}(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {{
            
        }}
");
    }

    private void AddProperties(StringBuilder builder)
    {
        builder.AppendLine("/*");
        foreach (var property in Properties)
        {
            if (property.IsReadOnly)
            {
                builder.AppendLine($@"        public {property} {{ get; init; }}");
            }
            else
            {
                builder.AppendLine($@"        public {property} {{ get; set; }}");
            }
        }
        builder.AppendLine("*/");
    }
}