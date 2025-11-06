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
            string sourceText = _fileService.ReadAllText(options.InputPath);

            var semanticModel = _compilationService.CreateSemanticModel(sourceText);

            var tree = semanticModel.SyntaxTree;
            var root = tree.GetRoot();

            var rewriter = new DuplicateSingleParameterRewriter(semanticModel, _nameSuggester, _methodAnalysisService);
            var newRoot = rewriter.Visit(root);

            var formatted = newRoot.NormalizeWhitespace(elasticTrivia: true).ToFullString();

            string outputPath = options.OutputPath ?? (options.OverwriteInput ? options.InputPath : options.InputPath);

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

