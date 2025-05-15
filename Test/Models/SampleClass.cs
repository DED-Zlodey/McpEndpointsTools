namespace Test.Models;

/// <summary>
/// Represents a static class that provides various method examples,
/// including methods with different parameter types and counts,
/// as well as a nested class with its own functionality.
/// </summary>
public static class SampleClass
{
    /// <summary>
    /// Executes a static parameterless method with no return value.
    /// </summary>
    /// <remarks>
    /// This method does not accept any arguments and performs a predefined task.
    /// It is part of the <c>SampleClass</c> in the <c>Test.Models</c> namespace.
    /// </remarks>
    public static void NoParams() { }

    /// <summary>
    /// Executes a static method that accepts a single integer parameter.
    /// </summary>
    /// <param name="x">An integer parameter used within the method.</param>
    public static void WithOneParam(int x) { }

    /// <summary>
    /// Performs an operation using two parameters of different types.
    /// </summary>
    /// <param name="a">The first parameter of type <c>string</c>, representing textual input.</param>
    /// <param name="b">The second parameter of type <c>double</c>, representing a numeric input.</param>
    public static void WithTwoParams(string a, double b) { }

    /// <summary>
    /// A method that operates on a list of integers.
    /// </summary>
    /// <param name="items">A list of integers to be processed by the method.</param>
    public static void WithGeneric(List<int> items) { }

    /// <summary>
    /// Represents a nested class within the <see cref="SampleClass"/>.
    /// </summary>
    /// <remarks>
    /// The <c>Nested</c> class is an inner class within <see cref="SampleClass"/>.
    /// It demonstrates the nesting of classes and encapsulates additional functionality.
    /// </remarks>
    public class Nested
    {
        /// <summary>
        /// A method defined within a nested class of the <c>SampleClass</c>.
        /// </summary>
        /// <remarks>
        /// This method is part of the <c>SampleClass.Nested</c> class hierarchy.
        /// It is designed to demonstrate the structure and behavior of methods within nested classes,
        /// and how they are referenced in XML documentation hierarchies.
        /// </remarks>
        public void InnerMethod()
        {
        }
    }
}
