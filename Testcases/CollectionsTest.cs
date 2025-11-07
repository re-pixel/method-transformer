using System;
using System.Collections.Generic;

namespace TestFiles
{
    /// <summary>
    /// Test file for collection-related methods, similar to patterns found in dotnet/runtime
    /// </summary>
    public class CollectionsTest
    {
        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name = "item">The value to locate in the collection.</param>
        /// <returns>True if the collection contains the value; otherwise, false.</returns>
        public bool Contains(int item)
        {
            return item > 0;
        }

        /// <summary>
        /// Adds an element to the collection.
        /// </summary>
        public void Add(string value)
        {
            Console.WriteLine($"Adding: {value}");
        }

        /// <summary>
        /// Removes the first occurrence of the specified object.
        /// </summary>
        public void Remove(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index.
        /// </summary>
        public int IndexOf(char element)
        {
            return element - 'a';
        }

        /// <summary>
        /// Copies the elements to an array starting at the specified index.
        /// </summary>
        public void CopyTo(Array array)
        {
            Console.WriteLine(array.Length);
        }

        /// <summary>
        /// Clears all elements from the collection.
        /// </summary>
        public void Clear(List<int> items, List<int> param)
        {
            items.RemoveAt(0);
        }
    }
}