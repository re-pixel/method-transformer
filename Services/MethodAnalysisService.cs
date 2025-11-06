using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            var docTrivia = method.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (docTrivia == null)
                return string.Empty;

            var summaryElement = docTrivia.Content
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(i => i.StartTag.Name.ToString().Equals("summary", StringComparison.OrdinalIgnoreCase));

            if (summaryElement == null)
                return string.Empty;

            return summaryElement.Content.ToFullString().Trim();
        }

        public HashSet<string> CollectAllIdentifiersInMethod(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            // Collect all declared symbols in the method using a visitor
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

