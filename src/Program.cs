using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    internal class Program
    {
        private static void Main(string[] args)
        {
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            IEnumerable<string> inputs = Directory.EnumerateFiles(Path.Combine(currentDirectory, "data"));

            Compilation compilation = CSharpCompilation
                .Create(nameof(RoslynSandbox))
                .WithReferences(mscorlib)
                .WithSourceFiles(inputs);

            new RoslynSandboxSymbolVisitor().Visit(compilation.Assembly.GlobalNamespace);
        }
    }
}