using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisTool.NameSuggestion;
using CodeAnalysisTool.Services;

namespace CodeAnalysisTool.Rewriters
{
    /// <summary>
    /// Rewriter that finds MethodDeclarationSyntax nodes with exactly one parameter,
    /// duplicates that parameter and gives the duplicate a suggested (unique) name.
    /// </summary>
    public class DuplicateSingleParameterRewriter : CSharpSyntaxRewriter
    {
        public bool FoundAny { get; private set; } = false;
        public int ChangesCount { get; private set; } = 0;

        private readonly INameSuggester _nameSuggester;
        private readonly SemanticModel _semanticModel;
        private readonly IMethodAnalysisService _methodAnalysisService;

        public DuplicateSingleParameterRewriter(
            SemanticModel semanticModel,
            INameSuggester nameSuggester,
            IMethodAnalysisService methodAnalysisService)
        {
            _nameSuggester = nameSuggester;
            _semanticModel = semanticModel;
            _methodAnalysisService = methodAnalysisService;
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;

            var parameters = node.ParameterList.Parameters;
            if (parameters.Count != 1)
                return node;

            FoundAny = true;

            var originalParam = parameters[0];

            string transformerContext = _methodAnalysisService.BuildTransformerContext(node, originalParam, _semanticModel);
            var existingNames = _methodAnalysisService.CollectAllIdentifiersInMethod(node, _semanticModel);
            
            Console.WriteLine(string.Join(", ", existingNames));
            Console.WriteLine(originalParam.Identifier.Text);

            var typeSymbol = _semanticModel.GetTypeInfo(originalParam.Type!).Type;
            string typeName = typeSymbol?.ToDisplayString() ?? "object";

            var suggested = _nameSuggester.SuggestName(originalParam.Identifier.Text, transformerContext, typeName, existingNames);
            if (string.IsNullOrWhiteSpace(suggested) || !SyntaxFacts.IsValidIdentifier(suggested))
            {
                suggested = "param";
            }

            var newIdentifier = SyntaxFactory.Identifier(suggested);
            var newParam = originalParam.WithIdentifier(newIdentifier.WithTriviaFrom(originalParam.Identifier));
            var newParams = parameters.Add(newParam);

            var newParamList = node.ParameterList.WithParameters(newParams);
            var updatedNode = node.WithParameterList(newParamList);

            var parameterSymbol = _semanticModel.GetDeclaredSymbol(originalParam) as IParameterSymbol;
            if (parameterSymbol != null && node.Body != null)
            {
                var firstUsage = FindFirstParameterUsage(node, originalParam.Identifier.Text, parameterSymbol);
                if (firstUsage != null && updatedNode.Body != null && node.Body != null)
                {
                    var originalStatements = node.Body.Statements;
                    var statementIndex = originalStatements.IndexOf(firstUsage);

                    if (statementIndex >= 0)
                    {
                        var clonedStatement = CloneStatementWithNewParameter(firstUsage, originalParam.Identifier.Text, suggested);

                        var updatedStatements = updatedNode.Body.Statements;
                        var newStatements = updatedStatements.Insert(statementIndex + 1, clonedStatement);
                        var newBody = updatedNode.Body.WithStatements(newStatements);
                        updatedNode = updatedNode.WithBody(newBody);
                    }
                }
            }

            ChangesCount++;
            return updatedNode;
        }

        /// <summary>
        /// Finds the first statement or expression in the method body where the parameter is used.
        /// </summary>
        private StatementSyntax? FindFirstParameterUsage(MethodDeclarationSyntax method, string parameterName, IParameterSymbol parameterSymbol)
        {
            if (method.Body == null)
                return null;

            foreach (var node in method.Body.DescendantNodes())
            {
                if (node is IdentifierNameSyntax identifierName)
                {
                    if (identifierName.Identifier.Text == parameterName)
                    {
                        var symbolInfo = _semanticModel.GetSymbolInfo(identifierName);
                        if (symbolInfo.Symbol != null &&
                            symbolInfo.Symbol.Equals(parameterSymbol, SymbolEqualityComparer.Default))
                        {
                            var statement = identifierName.FirstAncestorOrSelf<StatementSyntax>();
                            return statement;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Clones a statement and replaces all occurrences of the original parameter name with the new parameter name.
        /// </summary>
        private StatementSyntax CloneStatementWithNewParameter(StatementSyntax statement, string originalParamName, string newParamName)
        {
            var rewriter = new ParameterReplacer(originalParamName, newParamName, _semanticModel);
            return (StatementSyntax)rewriter.Visit(statement);
        }
    }
}

