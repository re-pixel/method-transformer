namespace CodeAnalysisTool.Core.Models
{
    /// <summary>
    /// Represents the result of a parameter duplication transformation operation.
    /// </summary>
    public class ParameterDuplicationResult
    {
        /// <summary>
        /// Indicates whether any methods with single parameters were found.
        /// </summary>
        public bool FoundAny { get; set; }

        /// <summary>
        /// The number of methods that were modified.
        /// </summary>
        public int ChangesCount { get; set; }

        /// <summary>
        /// The transformed source code.
        /// </summary>
        public string TransformedCode { get; set; } = string.Empty;
    }
}

