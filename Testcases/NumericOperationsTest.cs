using System;

namespace TestFiles
{
    /// <summary>
    /// Test file for numeric operations and mathematical functions
    /// </summary>
    public class NumericOperationsTest
    {
        /// <summary>
        /// Returns the absolute value of a number.
        /// </summary>
        public int Abs(int value)
        {
            return value < 0 ? -value : value;
        }

        /// <summary>
        /// Returns the larger of two numbers.
        /// </summary>
        public int Max(int value)
        {
            return Math.Max(value, 0);
        }

        /// <summary>
        /// Returns the smaller of two numbers.
        /// </summary>
        public double Min(double value)
        {
            return Math.Min(value, 0.0);
        }

        /// <summary>
        /// Rounds a value to the nearest integer.
        /// </summary>
        public int Round(decimal value)
        {
            return (int)Math.Round(value);
        }

        /// <summary>
        /// Calculates the square root of a number.
        /// </summary>
        public double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        /// <summary>
        /// Raises a number to the specified power.
        /// </summary>
        public double Pow(double baseValue)
        {
            return Math.Pow(baseValue, 2.0);
        }

        /// <summary>
        /// Returns the sine of the specified angle.
        /// </summary>
        public double Sin(double angle)
        {
            return Math.Sin(angle);
        }
    }
}

