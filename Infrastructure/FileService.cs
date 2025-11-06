using System.IO;

namespace CodeAnalysisTool.Infrastructure
{
    /// <summary>
    /// Service for file I/O operations.
    /// </summary>
    public class FileService : IFileService
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}

