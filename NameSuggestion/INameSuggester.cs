using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisTool.NameSuggestion
{
    /// <summary>
    /// Defines a contract for suggesting variable or parameter names 
    /// based on context and type information.
    /// </summary>
    public interface INameSuggester
    {
        /// <summary>
        /// Suggests one or more possible names for a new parameter or variable.
        /// </summary>
        /// <param name="context">String containing relevant code context (e.g. method name, body, nearby identifiers).</param>
        /// <param name="typeName">The data type of the parameter or variable.</param>
        /// <param name="count">Number of name suggestions to return. Default is 1.</param>
        /// <returns>A list of suggested names, sorted by relevance (best first).</returns>
        List<string> SuggestNames(string context, string typeName, ISet<string> existingNames, int count = 1);
        string SuggestName(string context, string typeName, ISet<string> existingNames);
    }
}
