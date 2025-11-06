using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTool.Rewriters
{
    /// <summary>
    /// Utility class for extracting XML documentation comments from syntax nodes.
    /// </summary>
    public static class DocumentationExtractor
    {
        /// <summary>
        /// Extracts the documentation comment summary from a method.
        /// </summary>
        public static string GetDocumentationComment(MethodDeclarationSyntax method)
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
    }
}

