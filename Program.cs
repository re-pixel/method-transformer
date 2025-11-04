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

            var nameSuggester = new NameSuggestion.HeuristicNameSuggester();
            var rewriter = new DuplicateSingleParameterRewriter(nameSuggester);
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

        public DuplicateSingleParameterRewriter(INameSuggester nameSuggester)
        {
            _nameSuggester = nameSuggester;
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;

            var parameters = node.ParameterList.Parameters;
            if (parameters.Count != 1)
                return node;

            FoundAny = true;

            var originalParam = parameters[0];

            var existingNames = parameters.Select(p => p.Identifier.Text).ToHashSet(StringComparer.Ordinal);

            Console.WriteLine(originalParam.Identifier.Text);
            var suggested = _nameSuggester.SuggestName(originalParam.Identifier.Text, originalParam.GetType().ToString(), existingNames);

            var newParam = originalParam.WithIdentifier(SyntaxFactory.Identifier(suggested)
                                                                    .WithTriviaFrom(originalParam.Identifier));
            var newParams = parameters.Add(newParam);

            var newParamList = node.ParameterList.WithParameters(newParams);

            ChangesCount++;
            return node.WithParameterList(newParamList);
        }
    }
}
