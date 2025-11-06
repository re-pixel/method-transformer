using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTool.Services
{
    /// <summary>
    /// Service for analyzing methods and extracting metadata.
    /// </summary>
    public interface IMethodAnalysisService
    {
        /// <summary>
        /// Extracts the documentation comment summary from a method.
        /// </summary>
        string GetDocumentationComment(MethodDeclarationSyntax method);

        /// <summary>
        /// Collects all identifier names that exist in the method scope to avoid naming collisions.
        /// </summary>
        HashSet<string> CollectAllIdentifiersInMethod(MethodDeclarationSyntax method, SemanticModel semanticModel);

        /// <summary>
        /// Builds a transformer context string from method information.
        /// </summary>
        string BuildTransformerContext(MethodDeclarationSyntax method, ParameterSyntax parameter, SemanticModel semanticModel);
    }
}

