using System;
using System.Text;

namespace TestFiles
{
    /// <summary>
    /// Test file for string manipulation methods, common in runtime string handling
    /// </summary>
    public class StringOperationsTest
    {
        /// <summary>
        /// Compares this string with another string and returns an integer.
        /// </summary>
        public int CompareTo(string other)
        {
            return string.Compare(this.ToString(), other);
        }

        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        public bool StartsWith(string value)
        {
            return value != null && this.ToString().StartsWith(value);
        }

        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        public bool EndsWith(string suffix)
        {
            return suffix != null && this.ToString().EndsWith(suffix);
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string are replaced.
        /// </summary>
        public string Replace(string oldValue)
        {
            return this.ToString().Replace(oldValue, string.Empty);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of characters.
        /// </summary>
        public string Trim(char[] trimChars)
        {
            return this.ToString().Trim(trimChars);
        }

        /// <summary>
        /// Splits a string into substrings based on the specified delimiter.
        /// </summary>
        public string[] Split(char separator)
        {
            return this.ToString().Split(separator);
        }

        /// <summary>
        /// Converts the string to a byte array using the specified encoding.
        /// </summary>
        public byte[] GetBytes(Encoding encoding)
        {
            return encoding.GetBytes(this.ToString());
        }
    }
}

