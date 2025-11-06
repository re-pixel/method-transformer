using CodeAnalysisTool.Core.Models;

namespace CodeAnalysisTool.Services
{
    /// <summary>
    /// Service for orchestrating code transformation operations.
    /// </summary>
    public interface ICodeTransformationService
    {
        /// <summary>
        /// Transforms a source file by duplicating single parameters in methods.
        /// </summary>
        ParameterDuplicationResult TransformFile(FileProcessingOptions options);
    }
}

