using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisTool.Rewriters;

namespace CodeAnalysisTool.Services
{
    /// <summary>
    /// Service for analyzing methods and extracting metadata.
    /// </summary>
    public class MethodAnalysisService : IMethodAnalysisService
    {
        public string GetDocumentationComment(MethodDeclarationSyntax method)
        {
            return DocumentationExtractor.GetDocumentationComment(method);
        }

        public HashSet<string> CollectAllIdentifiersInMethod(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            var collector = new SymbolCollector(semanticModel);
            collector.Visit(method);
            names.UnionWith(collector.DeclaredNames);

            return names;
        }

        public string BuildTransformerContext(MethodDeclarationSyntax method, ParameterSyntax parameter, SemanticModel semanticModel)
        {
            var typeSymbol = semanticModel.GetTypeInfo(parameter.Type!).Type;
            string typeName = typeSymbol?.ToDisplayString() ?? "object";
            string docComment = GetDocumentationComment(method);
            string methodName = method.Identifier.Text;

            return $"Method description: {docComment}\n" +
                   $"Type: {typeName} in method {methodName}";
        }
    }
}

