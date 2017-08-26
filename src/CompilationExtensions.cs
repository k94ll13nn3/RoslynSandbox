using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

public static class CompilationExtensions
{
    public static Compilation WithSourceFiles(this Compilation compilation, IEnumerable<string> inputs)
    {
        ICollection<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
        foreach (var input in inputs)
        {
            using (Stream stream = File.OpenRead(input))
            {
                SourceText sourceText = SourceText.From(stream);
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText));
            }
        }

        compilation = compilation.AddSyntaxTrees(syntaxTrees);

        return compilation;
    }
}