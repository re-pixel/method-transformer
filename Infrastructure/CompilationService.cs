using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalysisTool.Infrastructure
{
    /// <summary>
    /// Service for creating Roslyn compilations and semantic models.
    /// </summary>
    public class CompilationService : ICompilationService
    {
        public SemanticModel CreateSemanticModel(string sourceCode, string assemblyName = "CodeAnalysisAssembly")
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var references = GetDefaultReferences().ToList();

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: [tree],
                references: references);

            return compilation.GetSemanticModel(tree);
        }

        public IEnumerable<MetadataReference> GetDefaultReferences()
        {
            return
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            ];
        }
    }
}

