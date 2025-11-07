using System;
using System.Globalization;

namespace TestFiles
{
    /// <summary>
    /// Test file for type conversion and transformation methods
    /// </summary>
    public class ConversionTest
    {
        /// <summary>
        /// Converts the string representation of a number to its integer equivalent.
        /// </summary>
        public int ParseInt(string value)
        {
            return int.Parse(value);
        }

        /// <summary>
        /// Converts the string representation of a number to its double equivalent.
        /// </summary>
        public double ParseDouble(string value)
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the value to its string representation.
        /// </summary>
        public string ToString(int value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        public T ConvertTo<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Parses the string representation of a date and time.
        /// </summary>
        public DateTime ParseDateTime(string dateString)
        {
            return DateTime.Parse(dateString);
        }

        /// <summary>
        /// Converts the byte array to a base64 string.
        /// </summary>
        public string ToBase64String(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts the string from base64 to a byte array.
        /// </summary>
        public byte[] FromBase64String(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }
    }
}

