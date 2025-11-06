using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTool.Rewriters
{
    /// <summary>
    /// Visitor that collects all declared symbol names in a method using SemanticModel.
    /// This is more robust than manually checking syntax types.
    /// </summary>
    public class SymbolCollector : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        public HashSet<string> DeclaredNames { get; } = new HashSet<string>(StringComparer.Ordinal);

        public SymbolCollector(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            AddSymbolName(node);
            base.VisitParameter(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            AddSymbolName(node);
            base.VisitVariableDeclarator(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            AddSymbolName(node);
            base.VisitForEachStatement(node);
        }

        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            if (!node.Identifier.IsKind(SyntaxKind.None))
            {
                AddSymbolName(node);
            }
            base.VisitCatchDeclaration(node);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            DeclaredNames.Add(node.Identifier.Text);
            base.VisitLocalFunctionStatement(node);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            foreach (var param in node.ParameterList.Parameters)
            {
                AddSymbolName(param);
            }
            base.VisitParenthesizedLambdaExpression(node);
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            AddSymbolName(node.Parameter);
            base.VisitSimpleLambdaExpression(node);
        }

        private void AddSymbolName(SyntaxNode node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null && !string.IsNullOrEmpty(symbol.Name))
            {
                DeclaredNames.Add(symbol.Name);
            }
        }
    }
}

