using System;
using System.Collections.Generic;

namespace TestFiles
{
    /// <summary>
    /// Test file for comparison and equality checking methods
    /// </summary>
    public class ComparisonTest
    {
        /// <summary>
        /// Determines whether this instance equals the specified object.
        /// </summary>
        public bool Equals(object obj)
        {
            return obj != null && this.GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// Compares this instance with another object and returns an integer.
        /// </summary>
        public int CompareTo(IComparable other)
        {
            if (other == null) return 1;
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }

        /// <summary>
        /// Determines whether two sequences are equal by comparing elements.
        /// </summary>
        public bool SequenceEqual<T>(IEnumerable<T> other)
        {
            return Equals(other, this);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is greater than the specified value.
        /// </summary>
        public bool IsGreaterThan(int value)
        {
            return this.GetHashCode() > value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is less than the specified value.
        /// </summary>
        public bool IsLessThan(int value)
        {
            return this.GetHashCode() < value;
        }

        /// <summary>
        /// Determines if the collection contains any elements matching the predicate.
        /// </summary>
        public bool Any<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                if (item != null)
                    return true;
            }
            return false;
        }
    }
}

