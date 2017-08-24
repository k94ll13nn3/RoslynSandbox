using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

// see https://github.com/dotnet/roslyn/issues/6138
// see https://joshvarty.wordpress.com/2015/10/25/learn-roslyn-now-part-15-the-symbolvisitor/
// see https://github.com/Wyamio/Wyam/blob/develop/src/extensions/Wyam.CodeAnalysis/Analysis/AnalyzeSymbolVisitor.cs
// see https://stackoverflow.com/a/42075734/3836163
namespace RoslynSandbox
{
    public class RoslynSandboxSymbolVisitor : SymbolVisitor
    {
        public override void DefaultVisit(ISymbol symbol)
        {
            if (symbol is INamespaceOrTypeSymbol namespaceOrTypeSymbol)
            {
                foreach (var child in namespaceOrTypeSymbol.GetMembers())
                {
                    child.Accept(this);
                }
            }
            else
            {
                Console.WriteLine($"{symbol} ({(symbol is IMethodSymbol method ? method.MethodKind.ToString() : symbol.Kind.ToString())}): {IsInterfaceImplementation(symbol)}/{symbol.IsOverride}");
            }
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public bool IsInterfaceImplementation<T>(T method) where T : ISymbol
        {
            return method
                .ContainingType
                .AllInterfaces
                .SelectMany(@interface => @interface
                    .GetMembers()
                    .OfType<T>())
                .Any(interfaceMethod => method
                    .ContainingType
                    .FindImplementationForInterfaceMember(interfaceMethod)?.Equals(method) ?? false);
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            Compilation compilation = CSharpCompilation
                .Create(nameof(RoslynSandbox))
                .WithReferences(mscorlib);

            compilation = AddSourceFiles(compilation);

            new RoslynSandboxSymbolVisitor().Visit(compilation.Assembly.GlobalNamespace);
        }

        private static Compilation AddSourceFiles(Compilation compilation)
        {
            ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
            var inputs = Directory.EnumerateFiles(@"src");
            Parallel.ForEach(inputs, input =>
            {
                using (Stream stream = File.OpenRead(input))
                {
                    SourceText sourceText = SourceText.From(stream);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                        sourceText,
                        path: input));
                }
            });

            compilation = compilation.AddSyntaxTrees(syntaxTrees);

            return compilation;
        }
    }
}