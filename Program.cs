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
                Console.WriteLine("Usage: CodeAnalysisTool <input-file.cs> [output-file.cs]");
                return 1;
            }

            string inputPath = args[0];
            string? outputPath = args.Length >= 2 ? args[1] : null;

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

            var fileService = new FileService();
            var compilationService = new CompilationService();
            var methodAnalysisService = new MethodAnalysisService();

            var embeddingSuggester = new LocalEmbeddingSuggester("model/model.onnx", pineconeApiKey, "code-contexts");
            var mlNameSuggester = new MLNameSuggester(embeddingSuggester);

            if (!fileService.FileExists(inputPath))
            {
                Console.Error.WriteLine($"Error: input file not found: {inputPath}");
                return 2;
            }

            var transformationService = new CodeTransformationService(
                fileService,
                compilationService,
                mlNameSuggester,
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
