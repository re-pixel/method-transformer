using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisTool.NameSuggestion
{
    public class HeuristicNameSuggester : INameSuggester
    {
        public string SuggestName(string baseName, string context, string typeName, ISet<string> existingNames)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "param";

            string candidate;
            candidate = baseName + "2";
            if (!existingNames.Contains(candidate)) return candidate;

            candidate = baseName + "Copy";
            if (!existingNames.Contains(candidate)) return candidate;

            candidate = baseName + "_copy";
            if (!existingNames.Contains(candidate)) return candidate;

            for (int i = 1; i < 1000; i++)
            {
                candidate = baseName + "_" + i.ToString();
                if (!existingNames.Contains(candidate)) return candidate;
            }

            int suffix = 1;
            while (existingNames.Contains(baseName + "_dup" + suffix))
                suffix++;
            return baseName + "_dup" + suffix;
        }
        public List<string> SuggestNames(string originalName, string context, string typeName, ISet<string> existingNames, int count = 1)
        {
            return [SuggestName(originalName, context, typeName, existingNames)];
        }
    }
}
