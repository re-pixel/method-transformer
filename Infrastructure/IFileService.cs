namespace CodeAnalysisTool.Infrastructure
{
    /// <summary>
    /// Service for file I/O operations.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        string ReadAllText(string path);

        /// <summary>
        /// Writes all text to a file.
        /// </summary>
        void WriteAllText(string path, string contents);
    }
}

