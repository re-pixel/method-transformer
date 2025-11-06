using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CodeAnalysisTool.Core.Models;
using CodeAnalysisTool.Infrastructure;
using CodeAnalysisTool.NameSuggestion;
using CodeAnalysisTool.Rewriters;

namespace CodeAnalysisTool.Services
{
    /// <summary>
    /// Service for orchestrating code transformation operations.
    /// </summary>
    public class CodeTransformationService : ICodeTransformationService
    {
        private readonly IFileService _fileService;
        private readonly ICompilationService _compilationService;
        private readonly INameSuggester _nameSuggester;
        private readonly IMethodAnalysisService _methodAnalysisService;

        public CodeTransformationService(
            IFileService fileService,
            ICompilationService compilationService,
            INameSuggester nameSuggester,
            IMethodAnalysisService methodAnalysisService)
        {
            _fileService = fileService;
            _compilationService = compilationService;
            _nameSuggester = nameSuggester;
            _methodAnalysisService = methodAnalysisService;
        }

        public ParameterDuplicationResult TransformFile(FileProcessingOptions options)
        {
            // Read source file
            string sourceText = _fileService.ReadAllText(options.InputPath);

            // Create semantic model
            var semanticModel = _compilationService.CreateSemanticModel(sourceText);

            // Get syntax tree from semantic model (already parsed)
            var tree = semanticModel.SyntaxTree;
            var root = tree.GetRoot();

            // Apply rewriter
            var rewriter = new DuplicateSingleParameterRewriter(semanticModel, _nameSuggester, _methodAnalysisService);
            var newRoot = rewriter.Visit(root);

            // Format and get result
            var formatted = newRoot.NormalizeWhitespace(elasticTrivia: true).ToFullString();

            // Determine output path
            string outputPath = options.OutputPath ?? (options.OverwriteInput ? options.InputPath : options.InputPath);

            // Write output
            _fileService.WriteAllText(outputPath, formatted);

            return new ParameterDuplicationResult
            {
                FoundAny = rewriter.FoundAny,
                ChangesCount = rewriter.ChangesCount,
                TransformedCode = formatted
            };
        }
    }
}

