using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTool.EmbeddingCreation
{
    class ExtractTransformerContexts
    {
        private string _root = @"C:\Users\relja\source\ghrepos";
        private string _output = "contexts.json";
        private const int BatchSize = 10000;

        public ExtractTransformerContexts() { }

        public ExtractTransformerContexts(string rootPath, string outputPath)
        {
            _root = rootPath;
            _output = outputPath;
        }

        public async Task RunExtraction()
        {

            var files = Directory.EnumerateFiles(_root, "*.cs", SearchOption.AllDirectories)
                                     .Where(f => !f.Contains("obj") && !f.Contains("bin"))
                                     .ToList();

            Console.WriteLine($"Scanning {files.Count} files under {_root}...");

            var results = new ConcurrentBag<object>();

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
            {
                try
                {
                    var code = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(code)) return;

                    var tree = CSharpSyntaxTree.ParseText(code);
                    var rootNode = tree.GetRoot();

                    var methods = rootNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

                    foreach (var method in methods)
                    {
                        string methodName = method.Identifier.Text;

                        string summary = GetSummary(method);

                        foreach (var param in method.ParameterList.Parameters)
                        {
                            string type = param.Type?.ToString() ?? "object";
                            string paramName = param.Identifier.Text;

                            if (paramName.StartsWith("_") || paramName.Length < 2) continue;

                            string context = $"Method description: {summary}\nType: {type} in method {methodName}";

                            results.Add(new
                            {
                                transformerContext = context,
                                paramType = type,
                                paramName
                            });
                        }
                    }
                }
                catch { /* skip */ }
            });

            // Create contexts folder
            string contextsFolder = "contexts";
            Directory.CreateDirectory(contextsFolder);

            // Convert to list and batch
            var resultsList = results.ToList();
            int totalBatches = (int)Math.Ceiling((double)resultsList.Count / BatchSize);
            int batchNumber = 0;

            for (int i = 0; i < resultsList.Count; i += BatchSize)
            {
                var batchData = resultsList.Skip(i).Take(BatchSize).ToList();
                string batchFileName = Path.Combine(contextsFolder, $"contexts_{batchNumber}.json");
                var json = JsonSerializer.Serialize(batchData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(batchFileName, json);
                batchNumber++;
            }

            Console.WriteLine($"Extracted {results.Count} parameter contexts to {totalBatches} batch file(s) in {contextsFolder}");
        }

        private static string GetSummary(MethodDeclarationSyntax node)
        {
            var docTrivia = node.GetLeadingTrivia()
                .Select(t => t.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (docTrivia == null) return "";

            var summaryElement = docTrivia.Content
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(e => e.StartTag.Name.ToString().Equals("summary", StringComparison.OrdinalIgnoreCase));

            return summaryElement?.Content.ToFullString().Trim().Replace("\n", " ").Replace("\r", " ") ?? "";
        }
    }
}