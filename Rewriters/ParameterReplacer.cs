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
            if (node.Identifier.Text == _originalName)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is IParameterSymbol paramSymbol)
                {
                    return SyntaxFactory.IdentifierName(_newName)
                        .WithTriviaFrom(node);
                }
            }

            return base.VisitIdentifierName(node);
        }
    }
}

