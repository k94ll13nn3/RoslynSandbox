using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Strinken.Parser;

// see https://github.com/dotnet/roslyn/issues/6138
// see https://joshvarty.wordpress.com/2015/10/25/learn-roslyn-now-part-15-the-symbolvisitor/
// see https://github.com/Wyamio/Wyam/blob/develop/src/extensions/Wyam.CodeAnalysis/Analysis/AnalyzeSymbolVisitor.cs
// see https://stackoverflow.com/a/42075734/3836163
namespace RoslynSandbox
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string assembly = typeof(IToken).Assembly.Location;
            MetadataReference metadata = MetadataReference.CreateFromFile(assembly);
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Compilation compilation = CSharpCompilation
                .Create(nameof(RoslynSandbox))
                .WithReferences(mscorlib, metadata);

            IAssemblySymbol assemblySymbol = compilation.GetAssemblyOrModuleSymbol(metadata) as IAssemblySymbol;

            new RoslynSandboxSymbolVisitor().Visit(assemblySymbol.GlobalNamespace);
        }
    }
}