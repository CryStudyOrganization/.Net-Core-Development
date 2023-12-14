using System;
using System.Linq;

namespace Lab4
{
    public sealed class GenericMethods
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static T FindMax<T>(T a, T b, T c) where T : IComparable<T>
        {
            if (b.CompareTo(a) > 0) a = b;
            if (c.CompareTo(a) > 0) a = c;
            return a;
        }

        public static T Sum<T>(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            dynamic result = default(T)!;
            bool found = false;

            foreach (T item in collection)
            {
                result += item;
                if (!found) 
                    found = true;
            }

            if (!found)
                throw new InvalidOperationException("Collection is empty.");

            return result;
        }
    }

    class Program
    {
        static void Main()
        {
            int a = 5, b = 10;
            Console.WriteLine($"Before Swap: a = {a}, b = {b}");
            GenericMethods.Swap(ref a, ref b);
            Console.WriteLine($"After Swap: a = {a}, b = {b}");

            int maxInt = GenericMethods.FindMax(5, 10, 7);
            Console.WriteLine($"Max Integer: {maxInt}");

            List<int> numbers = new() { 1, 2, 3, 4, 5 };
            int sum = GenericMethods.Sum(numbers);
            Console.WriteLine($"Integer Sum: {sum}");
        }
    }
}