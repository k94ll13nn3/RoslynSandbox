using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynTestLibrary;

namespace RoslynSymbols
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MetadataReference testAssembly = MetadataReference.CreateFromFile(typeof(Data).Assembly.Location);
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Compilation compilation = CSharpCompilation
                .Create(nameof(RoslynSymbols))
                .WithReferences(mscorlib, testAssembly);

            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(testAssembly) as IAssemblySymbol;

            new RoslynSymbolVisitor().Visit(assemblySymbol.GlobalNamespace);
        }
    }
}