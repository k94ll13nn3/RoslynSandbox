using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynTestLibrary;

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
            MetadataReference testAssembly = MetadataReference.CreateFromFile(typeof(Data).Assembly.Location);
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Compilation compilation = CSharpCompilation
                .Create(nameof(RoslynSandbox))
                .WithReferences(mscorlib, testAssembly);

            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(testAssembly) as IAssemblySymbol;

            new RoslynSandboxSymbolVisitor().Visit(assemblySymbol.GlobalNamespace);
        }
    }
}