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

       public List<string> SuggestNames(string originalName, string context, string typeName, ISet<string> existingNames, int count = 5)
       {
           var suggestions = _embeddingModel.GetNameSuggestions(context, typeName, count).Result;
           for(int i = 0; i < suggestions.Count; i++)
           {
                suggestions[i] = suggestions[i].Trim().Trim('"', '\'', '`');
           }
           var filtered = suggestions.Where(name => !existingNames.Contains(name)).Take(count).ToList();
           
            return filtered;
       }

       public string SuggestName(string originalName, string context, string typeName, ISet<string> existingNames)
       {
            var suggestions = SuggestNames(originalName, context, typeName, existingNames, 5);
           if (suggestions.Count > 0)
               return suggestions[0];
           return "param";
       }
   }
}
