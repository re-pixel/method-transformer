using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestFiles
{
    /// <summary>
    /// Test file for edge cases and special scenarios
    /// </summary>
    public class EdgeCasesTest
    {
        // Empty method
        public void EmptyMethod(int value)
        {
        }

        // Method with only return statement
        public int ReturnOnly(int value)
        {
            return value;
        }

        // Method with void return
        public void VoidReturn(string message)
        {
            Console.WriteLine(message);
        }

        // Async method
        public async Task ProcessAsync(int id)
        {
            await Task.Delay(100);
        }

        // Method with generics
        public void ProcessGeneric<T>(T item)
        {
            Console.WriteLine(item);
        }

        // Method with nullable parameter
        public void ProcessNullable(int? value)
        {
            if (value.HasValue)
                Console.WriteLine(value.Value);
        }

        // Method with ref parameter (should not be transformed, but good to test)
        public void ProcessRef(ref int value)
        {
            value = 42;
        }

        // Method with out parameter (should not be transformed, but good to test)
        public void ProcessIn(in int value)
        {
        }

        // Method with default parameter value
        public void ProcessWithDefault(string name = "default")
        {
            Console.WriteLine(name);
        }

        // Method with attributes
        [Obsolete]
        public void ObsoleteMethod(int count)
        {
            Console.WriteLine(count);
        }

        // Method with multiple statements using the parameter
        public void MultipleStatements(int value)
        {
            value += 1;
            value *= 2;
            Console.WriteLine(value);
            if (value > 10)
                Console.WriteLine("Large value");
        }

        // Method with lambda expression
        public void WithLambda(Action<int> callback)
        {
            callback(42);
        }

        // Method with cancellation token (common in async operations)
        public Task ProcessWithCancellation(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

