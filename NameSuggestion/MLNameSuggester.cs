using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisTool.NameSuggestion
{
    internal class MLNameSuggester : INameSuggester
    {
        private readonly LocalEmbeddingSuggester _embeddingModel;

        public MLNameSuggester(LocalEmbeddingSuggester embeddingModel)
        {
            _embeddingModel = embeddingModel;
        }

        public List<string> SuggestNames(string context, string typeName, ISet<string> existingNames, int count = 1)
        {
            var suggestions = _embeddingModel.GetNameSuggestions(context, typeName, count);
            var filtered = suggestions.Where(name => !existingNames.Contains(name)).Take(count).ToList();
            return filtered;
        }

        public string SuggestName(string context, string typeName, ISet<string> existingNames)
        {
            var suggestions = SuggestNames(context, typeName, existingNames, 1);
            if (suggestions.Count > 0)
                return suggestions[0];
            return "param";
        }
    }
}
