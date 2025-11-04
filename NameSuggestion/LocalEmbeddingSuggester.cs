using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Text.Json;
using System.Numerics;
using AllMiniLmL6V2Sharp;

namespace CodeAnalysisTool.NameSuggestion
{
    internal class LocalEmbeddingSuggester : IDisposable
    {
        private readonly AllMiniLmL6V2Embedder _embedder;
        private readonly Dictionary<string, float[]> _nameEmbeddings = new();

        public LocalEmbeddingSuggester(string modelPath, string candidatesPath)
        {
            _embedder = new AllMiniLmL6V2Embedder(modelPath: modelPath);

            var json = File.ReadAllText(candidatesPath);
            var candidates = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);

            foreach (var typeGroup in candidates)
            {
                foreach (var name in typeGroup.Value)
                {
                    _nameEmbeddings[name] = EmbedText(name);
                }
            }
        }

        public List<string> GetNameSuggestions(string context, string type, int count)
        {
            var contextVec = EmbedText(context);
            var best = _nameEmbeddings
                .OrderByDescending(kv => CosineSimilarity(contextVec, kv.Value))
                .Take(count)
                .Select(kv => kv.Key)
                .ToList();

            return best;
        }

        private float[] EmbedText(string text)
        {
            return (float[])_embedder.GenerateEmbedding(text);
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }
            return dot / ((float)Math.Sqrt(normA) * (float)Math.Sqrt(normB));
        }

        public void Dispose() => _embedder.Dispose();
    }
}
