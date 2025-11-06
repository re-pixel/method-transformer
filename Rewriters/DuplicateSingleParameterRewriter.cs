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

            // Find the first usage of the original parameter and copy it with the new parameter
            var parameterSymbol = _semanticModel.GetDeclaredSymbol(originalParam) as IParameterSymbol;
            if (parameterSymbol != null && node.Body != null)
            {
                // Find usage in the original node (before parameter list update)
                var firstUsage = FindFirstParameterUsage(node, originalParam.Identifier.Text, parameterSymbol);
                if (firstUsage != null && updatedNode.Body != null && node.Body != null)
                {
                    // Find the index of the statement in the original body
                    var originalStatements = node.Body.Statements;
                    var statementIndex = originalStatements.IndexOf(firstUsage);

                    if (statementIndex >= 0)
                    {
                        // Clone the statement with the new parameter name
                        var clonedStatement = CloneStatementWithNewParameter(firstUsage, originalParam.Identifier.Text, suggested);

                        // Insert the cloned statement right after the original in the updated body
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

            // Traverse all descendant nodes to find identifier usages
            foreach (var node in method.Body.DescendantNodes())
            {
                if (node is IdentifierNameSyntax identifierName)
                {
                    // Check if the identifier matches the parameter name
                    if (identifierName.Identifier.Text == parameterName)
                    {
                        // Use semantic model to verify this refers to the parameter (not a local variable)
                        var symbolInfo = _semanticModel.GetSymbolInfo(identifierName);
                        if (symbolInfo.Symbol != null &&
                            symbolInfo.Symbol.Equals(parameterSymbol, SymbolEqualityComparer.Default))
                        {
                            // Find the containing statement
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
            // Create a rewriter to replace the parameter identifier
            var rewriter = new ParameterReplacer(originalParamName, newParamName, _semanticModel);
            return (StatementSyntax)rewriter.Visit(statement);
        }
    }
}

