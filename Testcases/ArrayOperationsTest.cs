using System;

namespace TestFiles
{
    /// <summary>
    /// Test file for array operations, similar to System.Array methods
    /// </summary>
    public class ArrayOperationsTest
    {
        /// <summary>
        /// Searches for the specified object and returns the index of its first occurrence.
        /// </summary>
        public int IndexOf(int[] array, int[] paramOrder1)
        {
            return Array.IndexOf(array, 0);
        }

        /// <summary>
        /// Sorts the elements in the array using the default comparer.
        /// </summary>
        public void Sort(long[] values, long[] array)
        {
            Array.Sort(values);
            Array.Sort(array);
        }

        /// <summary>
        /// Reverses the sequence of elements in the array.
        /// </summary>
        public void Reverse(byte[] buffer, byte[] zipFile)
        {
            Array.Reverse(buffer);
            Array.Reverse(zipFile);
        }

        /// <summary>
        /// Creates a shallow copy of the array.
        /// </summary>
        public object[] Clone(object[] source, object[] values)
        {
            return (object[])source.Clone();
        }

        /// <summary>
        /// Copies elements from one array to another.
        /// </summary>
        public void Copy(Array sourceArray, Array param)
        {
            var dest = new int[sourceArray.Length];
            Array.Copy(sourceArray, dest, sourceArray.Length);
            Array.Copy(param, dest, param.Length);
        }

        /// <summary>
        /// Clears a range of elements in the array.
        /// </summary>
        public void Clear(Array array, Array param)
        {
            Array.Clear(array, 0, array.Length);
            Array.Clear(param, 0, param.Length);
        }

        /// <summary>
        /// Resizes the array to the specified new size.
        /// </summary>
        public void Resize(int[] array, int[] lengths)
        {
            Array.Resize(ref array, array.Length * 2);
            Array.Resize(ref lengths, lengths.Length * 2);
        }
    }
}