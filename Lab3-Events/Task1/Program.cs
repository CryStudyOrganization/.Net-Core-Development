using System;

namespace Task1
{
    public class Calculator
    {
        public delegate void CalculationHandler(int result);
        public event CalculationHandler? CalculationCompleted;

        public void PerformCalculation(int a, int b)
        {
            int result = a + b;
            OnCalculationCompleted(result);
        }

        protected virtual void OnCalculationCompleted(int result)
        {
            CalculationCompleted?.Invoke(result);
        }
    }

    public class Timer
    {
        private DateTime startTime;

        public void Start()
        {
            startTime = DateTime.Now;
        }

        public TimeSpan Stop()
        {
            return DateTime.Now - startTime;
        }
    }

    public class TestClass
    {
        public static void Main()
        {
            Calculator calculator = new();
            Timer timer = new();

            calculator.CalculationCompleted += CalculationCompletedHandler;

            timer.Start();

            calculator.PerformCalculation(10, 15);

            TimeSpan elapsedTime = timer.Stop();

            Console.WriteLine("Calculation completed in {0} milliseconds.", elapsedTime.TotalMilliseconds);
        }

        public static void CalculationCompletedHandler(int result)
        {
            Console.WriteLine("Calculation result: " + result);
        }
    }
}
