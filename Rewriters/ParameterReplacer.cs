using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTool.Rewriters
{
    /// <summary>
    /// Helper rewriter to replace parameter identifier references in a syntax tree.
    /// </summary>
    public class ParameterReplacer : CSharpSyntaxRewriter
    {
        private readonly string _originalName;
        private readonly string _newName;
        private readonly SemanticModel _semanticModel;

        public ParameterReplacer(string originalName, string newName, SemanticModel semanticModel)
        {
            _originalName = originalName;
            _newName = newName;
            _semanticModel = semanticModel;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            // Check if this identifier matches the original parameter name
            if (node.Identifier.Text == _originalName)
            {
                // Verify it's actually the parameter (not a local variable)
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is IParameterSymbol paramSymbol)
                {
                    // Replace with new parameter name
                    return SyntaxFactory.IdentifierName(_newName)
                        .WithTriviaFrom(node);
                }
            }

            return base.VisitIdentifierName(node);
        }
    }
}

