namespace CodeAnalysisTool.Core.Models
{
    /// <summary>
    /// Options for file processing operations.
    /// </summary>
    public class FileProcessingOptions
    {
        /// <summary>
        /// The path to the input file to process.
        /// </summary>
        public string InputPath { get; set; } = string.Empty;

        /// <summary>
        /// The path to the output file. If not specified, the input file will be overwritten.
        /// </summary>
        public string? OutputPath { get; set; }

        /// <summary>
        /// Whether to overwrite the input file if no output path is specified.
        /// </summary>
        public bool OverwriteInput { get; set; } = true;
    }
}

