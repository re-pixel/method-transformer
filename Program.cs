using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisTool.NameSuggestion;
using DotNetEnv;

namespace CodeAnalysisTool
{
    class Program
    {
        static int Main(string[] args)
        {
            // Load environment variables from .env file
            Env.Load();

            // var embeddingExtractor = new ExtractTransformerContexts();
            // embeddingExtractor.RunExtraction().Wait();

            // Get Pinecone API key from environment variable
            var pineconeApiKey = Environment.GetEnvironmentVariable("PINECONE_API_KEY");
            if (string.IsNullOrEmpty(pineconeApiKey))
            {
                Console.Error.WriteLine("Error: PINECONE_API_KEY environment variable is not set. Please create a .env file with PINECONE_API_KEY=your_key");
                return 1;
            }

            var embeddingSuggester = new LocalEmbeddingSuggester("model/model.onnx", pineconeApiKey, "code-contexts");
            //embeddingSuggester.PopulateVectorBaseAsync("contexts").Wait();
            var mlNameSuggester = new MLNameSuggester(embeddingSuggester);


            if (args.Length == 0)
            {
                Console.WriteLine("Usage: SyntaxTreeManualTraversal <input-file.cs> [output-file.cs]");
                return 1;
            }

            string inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: input file not found: {inputPath}");
                return 2;
            }

            string outputPath = args.Length >= 2 ? args[1] : inputPath; // overwrite by default

            string sourceText = File.ReadAllText(inputPath);

            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetRoot();

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                "CodeAnalysisAssembly",
                syntaxTrees: new[] { tree },
                references: references);

            var semanticModel = compilation.GetSemanticModel(tree);

            var heuristicNameSuggester = new HeuristicNameSuggester();
            var rewriter = new DuplicateSingleParameterRewriter(semanticModel, mlNameSuggester);
            var newRoot = rewriter.Visit(root);

            if (!rewriter.FoundAny)
            {
                Console.WriteLine("No method declarations with a single parameter were found. No changes made.");
                return 0;
            }

            var formatted = newRoot.NormalizeWhitespace(elasticTrivia: true).ToFullString();

            File.WriteAllText(outputPath, formatted);

            Console.WriteLine($"Processed file. Methods changed: {rewriter.ChangesCount}. Output written to: {outputPath}");
            return 0;
        }
    }

    /// <summary>
    /// Rewriter that finds MethodDeclarationSyntax nodes with exactly one parameter,
    /// duplicates that parameter and gives the duplicate a suggested (unique) name.
    /// </summary>
    internal class DuplicateSingleParameterRewriter : CSharpSyntaxRewriter
    {
        public bool FoundAny { get; private set; } = false;
        public int ChangesCount { get; private set; } = 0;

        private readonly INameSuggester _nameSuggester;
        private readonly SemanticModel _semanticModel;

        public DuplicateSingleParameterRewriter(SemanticModel semanticModel, INameSuggester nameSuggester)
        {
            _nameSuggester = nameSuggester;
            _semanticModel = semanticModel;
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;

            var parameters = node.ParameterList.Parameters;
            if (parameters.Count != 1)
                return node;

            FoundAny = true;

            var originalParam = parameters[0];

            var typeSymbol = _semanticModel.GetTypeInfo(originalParam.Type!).Type;
            string typeName = typeSymbol?.ToDisplayString() ?? "object";
            string docComment = GetDocumentationComment(node);
            string methodName = node.Identifier.Text;

            string transformerContext = $"Method description: {docComment}\n" +
                            $"Type: {typeName} in method {methodName}";

            var existingNames = CollectAllIdentifiersInMethod(node);
            Console.WriteLine(string.Join(", ", existingNames));

            Console.WriteLine(originalParam.Identifier.Text);
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

        private string GetDocumentationComment(MethodDeclarationSyntax node)
        {
            var docTrivia = node.GetLeadingTrivia()
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

        /// <summary>
        /// Collects all identifier names that exist in the method scope to avoid naming collisions.
        /// </summary>
        private HashSet<string> CollectAllIdentifiersInMethod(MethodDeclarationSyntax method)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            // Add all parameter names
            foreach (var param in method.ParameterList.Parameters)
            {
                names.Add(param.Identifier.Text);
            }

            if (method.Body == null)
                return names;

            // Collect all local variable names
            foreach (var node in method.Body.DescendantNodes())
            {
                // Local variable declarations: int x = 5;
                if (node is VariableDeclaratorSyntax variableDeclarator)
                {
                    names.Add(variableDeclarator.Identifier.Text);
                }
                // For loop variables: for (int i = 0; ...)
                else if (node is ForStatementSyntax forStatement)
                {
                    foreach (var declaration in forStatement.Declaration?.Variables ?? Enumerable.Empty<VariableDeclaratorSyntax>())
                    {
                        names.Add(declaration.Identifier.Text);
                    }
                }
                // Foreach loop variables: foreach (var item in items)
                else if (node is ForEachStatementSyntax foreachStatement)
                {
                    names.Add(foreachStatement.Identifier.Text);
                }
                // Catch clause variables: catch (Exception ex)
                else if (node is CatchDeclarationSyntax catchDeclaration && !catchDeclaration.Identifier.IsKind(SyntaxKind.None))
                {
                    names.Add(catchDeclaration.Identifier.Text);
                }
                // Using statement variables: using (var stream = ...)
                else if (node is UsingStatementSyntax usingStatement)
                {
                    if (usingStatement.Declaration != null)
                    {
                        foreach (var variable in usingStatement.Declaration.Variables)
                        {
                            names.Add(variable.Identifier.Text);
                        }
                    }
                }
            }

            // Also collect lambda parameters and local functions
            foreach (var node in method.DescendantNodes())
            {
                // Lambda parameters: (x, y) => ...
                if (node is LambdaExpressionSyntax lambda)
                {
                    if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
                    {
                        foreach (var param in parenthesizedLambda.ParameterList.Parameters)
                        {
                            names.Add(param.Identifier.Text);
                        }
                    }
                    else if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
                    {
                        names.Add(simpleLambda.Parameter.Identifier.Text);
                    }
                }
                // Local function parameters
                else if (node is LocalFunctionStatementSyntax localFunction)
                {
                    foreach (var param in localFunction.ParameterList.Parameters)
                    {
                        names.Add(param.Identifier.Text);
                    }
                    names.Add(localFunction.Identifier.Text);
                }
            }

            return names;
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

        /// <summary>
        /// Helper rewriter to replace parameter identifier references in a syntax tree.
        /// </summary>
        private class ParameterReplacer : CSharpSyntaxRewriter
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
}
