using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisTool.Infrastructure
{
    /// <summary>
    /// Service for creating Roslyn compilations and semantic models.
    /// </summary>
    public interface ICompilationService
    {
        /// <summary>
        /// Creates a compilation and semantic model from source code.
        /// </summary>
        /// <param name="sourceCode">The source code to compile.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <returns>The semantic model for the compilation.</returns>
        SemanticModel CreateSemanticModel(string sourceCode, string assemblyName = "CodeAnalysisAssembly");

        /// <summary>
        /// Gets the default metadata references for compilation.
        /// </summary>
        IEnumerable<MetadataReference> GetDefaultReferences();
    }
}

