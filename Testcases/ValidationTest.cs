using System;

namespace TestFiles
{
    /// <summary>
    /// Test file for validation and argument checking methods
    /// </summary>
    public class ValidationTest
    {
        /// <summary>
        /// Validates that the argument is not null.
        /// </summary>
        public void ValidateNotNull(object argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// Validates that the string is not null or empty.
        /// </summary>
        public void ValidateNotEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
        }

        /// <summary>
        /// Validates that the number is within the specified range.
        /// </summary>
        public void ValidateRange(int index)
        {
            if (index < 0 || index > 100)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Checks if the condition is true, otherwise throws an exception.
        /// </summary>
        public void AssertTrue(bool condition)
        {
            if (!condition)
                throw new InvalidOperationException("Condition must be true");
        }

        /// <summary>
        /// Ensures the array is not null and has at least one element.
        /// </summary>
        public void ValidateArray(int[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array cannot be null or empty", nameof(array));
        }

        /// <summary>
        /// Verifies that the enum value is valid.
        /// </summary>
        public void ValidateEnum<T>(T enumValue) where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), enumValue))
                throw new ArgumentException($"Invalid enum value: {enumValue}");
        }
    }
}

