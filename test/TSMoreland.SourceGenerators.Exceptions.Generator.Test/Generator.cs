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

using System.Collections.Immutable;
using System.Reflection;

namespace TSMoreland.SourceGenerators.Exceptions.Generator.Test;

internal static class Generator
{
     public static GeneratorOutput Compile(string source)
     {
        Compilation compilation = CreateCompilation(source);

         ImmutableArray<ISourceGenerator> generators = ImmutableArray.Create<ISourceGenerator>(new ExceptionGenerator());
         CSharpParseOptions options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
         GeneratorDriver driver =
             CSharpGeneratorDriver.Create(generators, ImmutableArray<AdditionalText>.Empty, options, null);

         driver.RunGeneratorsAndUpdateCompilation(
             compilation,
             out Compilation updatedCompilation,
             out ImmutableArray<Diagnostic> diagnostics);

         return new GeneratorOutput(driver, updatedCompilation, diagnostics);
     }
 
     private static Compilation CreateCompilation(string source)
     {
         string assemblyName = string.Empty;
         SyntaxTree[] syntaxTrees = new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10)) };
         PortableExecutableReference[] metadataReferences = new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) };
         CSharpCompilationOptions options = new(OutputKind.ConsoleApplication);
 
         return CSharpCompilation.Create(assemblyName, syntaxTrees, metadataReferences, options);
     }
}
