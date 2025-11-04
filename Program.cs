using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SyntaxTreeManualTraversal
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

            var rewriter = new DuplicateSingleParameterRewriter();
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

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // visit children first (so nested methods or local functions are processed separately if needed)
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;

            var parameters = node.ParameterList.Parameters;
            if (parameters.Count != 1)
                return node;

            FoundAny = true;

            var originalParam = parameters[0];

            var existingNames = parameters.Select(p => p.Identifier.Text).ToHashSet(StringComparer.Ordinal);

            var suggested = SuggestNewParameterName(originalParam.Identifier.Text, existingNames);

            var newParam = originalParam.WithIdentifier(SyntaxFactory.Identifier(suggested)
                                                                    .WithTriviaFrom(originalParam.Identifier));

            var newParams = parameters.Add(newParam);

            var newParamList = node.ParameterList.WithParameters(newParams);

            ChangesCount++;
            return node.WithParameterList(newParamList);
        }

        /// <summary>
        /// Suggests a new name for a duplicated parameter based on an existing name.
        /// Strategy:
        ///  - Try "<name>2", "<name>Copy", "<name>_copy", "<name>New", "<name>_1", ...
        ///  - Ensure there is no collision with 'existingNames'.
        /// </summary>
        private static string SuggestNewParameterName(string baseName, ISet<string> existingNames)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "param";

            string candidate;
            candidate = baseName + "2";
            if (!existingNames.Contains(candidate)) return candidate;

            candidate = baseName + "Copy";
            if (!existingNames.Contains(candidate)) return candidate;

            candidate = baseName + "_copy";
            if (!existingNames.Contains(candidate)) return candidate;

            for (int i = 1; i < 1000; i++)
            {
                candidate = baseName + "_" + i.ToString();
                if (!existingNames.Contains(candidate)) return candidate;
            }

            int suffix = 1;
            while (existingNames.Contains(baseName + "_dup" + suffix))
                suffix++;
            return baseName + "_dup" + suffix;
        }
    }
}
