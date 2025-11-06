namespace CodeAnalysisTool.Infrastructure
{
    /// <summary>
    /// Service for managing configuration and environment variables.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the Pinecone API key from environment variables.
        /// </summary>
        /// <returns>The API key if found, otherwise null.</returns>
        string? GetPineconeApiKey();

        /// <summary>
        /// Validates that the Pinecone API key is set.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        bool ValidatePineconeApiKey();
    }
}

