using System;
using DotNetEnv;

namespace CodeAnalysisTool.Infrastructure
{
    /// <summary>
    /// Service for managing configuration and environment variables.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly string? _pineconeApiKey;

        public ConfigurationService()
        {
            // Load environment variables from .env file
            Env.Load();
            _pineconeApiKey = Environment.GetEnvironmentVariable("PINECONE_API_KEY");
        }

        public string? GetPineconeApiKey()
        {
            return _pineconeApiKey;
        }

        public bool ValidatePineconeApiKey()
        {
            if (string.IsNullOrEmpty(_pineconeApiKey))
            {
                Console.Error.WriteLine("Error: PINECONE_API_KEY environment variable is not set. Please create a .env file with PINECONE_API_KEY=your_key");
                return false;
            }
            return true;
        }
    }
}

