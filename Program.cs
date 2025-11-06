using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisTool.NameSuggestion;

namespace CodeAnalysisTool
{
    class Program
    {
        static int Main(string[] args)
        {
            // var embeddingExtractor = new ExtractTransformerContexts();
            // embeddingExtractor.RunExtraction().Wait();

            var embeddingSuggester = new LocalEmbeddingSuggester("model/model.onnx", "pcsk_43jLyU_7fsqgATj5VXCymQVgZ2Yb7WR4v3YNLac2eoGMVqgsVL6mqYyFQwd6d6WYUyw4Ut", "code-contexts");
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

            var existingNames = parameters.Select(p => p.Identifier.Text).ToHashSet(StringComparer.Ordinal);

            Console.WriteLine(originalParam.Identifier.Text);
            var suggested = _nameSuggester.SuggestName(originalParam.Identifier.Text, transformerContext, typeName, existingNames);
            Console.WriteLine(existingNames.ToList()[0]);
            

            if (!string.IsNullOrWhiteSpace(suggested))
            {
                suggested = suggested.Trim().Trim('"', '\'', '`');
            }
            
            if (string.IsNullOrWhiteSpace(suggested) || !SyntaxFacts.IsValidIdentifier(suggested))
            {
                suggested = "param";
            }

            
            var newIdentifier = SyntaxFactory.Identifier(suggested);
            var newParam = originalParam.WithIdentifier(newIdentifier.WithTriviaFrom(originalParam.Identifier));
            var newParams = parameters.Add(newParam);

            var newParamList = node.ParameterList.WithParameters(newParams);

            ChangesCount++;
            return node.WithParameterList(newParamList);
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
    }
}
