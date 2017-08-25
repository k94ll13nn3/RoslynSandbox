using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

public static class CompilationExtensions
{
    public static Compilation WithSourceFiles(this Compilation compilation, IEnumerable<string> inputs)
    {
        ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
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