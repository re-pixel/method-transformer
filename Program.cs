using System;
using CodeAnalysisTool.Core.Models;
using CodeAnalysisTool.Infrastructure;
using CodeAnalysisTool.NameSuggestion;
using CodeAnalysisTool.Services;

namespace CodeAnalysisTool
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: CodeAnalysisTool <input-file.cs> [output-file.cs] [--suggester=heuristic|semantic]");
                Console.WriteLine("  --suggester: Choose name suggester (heuristic or semantic). Default: semantic");
                return 1;
            }

            string inputPath = args[0];
            string? outputPath = null;
            string suggesterType = "semantic";

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("--suggester="))
                {
                    suggesterType = args[i].Substring("--suggester=".Length).ToLowerInvariant();
                }
                else if (!args[i].StartsWith("--"))
                {
                    if (outputPath == null)
                    {
                        outputPath = args[i];
                    }
                }
            }

            if (suggesterType != "heuristic" && suggesterType != "semantic")
            {
                Console.Error.WriteLine($"Error: Invalid suggester type '{suggesterType}'. Use 'heuristic' or 'semantic'.");
                return 1;
            }

            var fileService = new FileService();
            var compilationService = new CompilationService();
            var methodAnalysisService = new MethodAnalysisService();

            INameSuggester nameSuggester;

            if (suggesterType == "semantic")
            {
                var configurationService = new ConfigurationService();
                if (!configurationService.ValidatePineconeApiKey())
                {
                    return 1;
                }

                var pineconeApiKey = configurationService.GetPineconeApiKey();
                if (string.IsNullOrEmpty(pineconeApiKey))
                {
                    return 1;
                }

                var embeddingSuggester = new LocalEmbeddingSuggester("model/model.onnx", pineconeApiKey, "code-contexts");
                nameSuggester = new MLNameSuggester(embeddingSuggester);
            }
            else
            {
                nameSuggester = new HeuristicNameSuggester();
            }

            if (!fileService.FileExists(inputPath))
            {
                Console.Error.WriteLine($"Error: input file not found: {inputPath}");
                return 2;
            }

            var transformationService = new CodeTransformationService(
                fileService,
                compilationService,
                nameSuggester,
                methodAnalysisService);

            var options = new FileProcessingOptions
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                OverwriteInput = outputPath == null
            };

            var result = transformationService.TransformFile(options);

            if (!result.FoundAny)
            {
                Console.WriteLine("No method declarations with a single parameter were found. No changes made.");
                return 0;
            }

            string outputFile = outputPath ?? inputPath;
            Console.WriteLine($"Processed file. Methods changed: {result.ChangesCount}. Output written to: {outputFile}");
            return 0;
        }
    }
}
