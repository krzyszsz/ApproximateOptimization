using System;
using System.Linq;

namespace ApproximateOptimization
{
    class Program
    {
        private static void Example1_FindMaximum()
        {
            // Finds maximum of sin(x) * cos(y) for range x: 0..1 and y: 0..1 (there are easy solutions without the solver, this is just an example)
            var func = (double[] vector) => Math.Sin(vector[0]) * Math.Cos(vector[1]);
            var optimizer = new CompositeOptimizer();
            optimizer.FindMaximum(2, func, maxIterations: 100); // 2 means that we have 2 dimensions (i.e. 2 numbers in vector in the function)
            Console.WriteLine($"Example1: maximum value {optimizer.SolutionValue} was found for x={optimizer.BestSolutionSoFar[0]} and y={optimizer.BestSolutionSoFar[1]} for x&y in range 0..1.");
            // This prints:
            // Example1: maximum value 0.8414709848078965 was found for x=1 and y=0 for x&y in range 0..1.
        }

        struct Point { public double x; public double y; }

        private static void Example2_Linear_regression()
        {
            // Finds simple regression line for a couple of points. To do that we need to minimize the sum of vertical distances to our line (there are faster ways than using a solver, this is just an example).
            // This obviously can use more complex regression models, not only lines (imagine: trygonometric functions if we wnated to make a slow version of discrete frequency transformation).
            var points = new Point[] {
                new Point{ x = 13.2, y = 2.3 },
                new Point{ x = 23.3, y = 3.4 },
                new Point{ x = 33.6, y = 4.1 },
                new Point{ x = 34.7, y = 5.3 },
            };
            var regressionLine = (double[] coefficients, Point point) => (coefficients[0] * point.x) + coefficients[1];
            var errorFunction = (double[] coefficients) => points.Sum(point => Math.Abs(regressionLine(coefficients, point) - point.y));
            var reverseErrorFunction = (double[] coefficients) => 1 / (errorFunction(coefficients) + 1);    // We need to reverse the function to maximize it rather than minimize it.
            var optimizer = new CompositeOptimizer();
            optimizer.FindMaximum(2, reverseErrorFunction, maxIterations: 100); // 2 means that we have 2 dimensions (i.e. 2 numbers in vector in the function)
            Console.WriteLine($"Example2: Found regression line y = {optimizer.BestSolutionSoFar[0] :N4}*x + {optimizer.BestSolutionSoFar[1] :N4}");
            // This prints:
            // Example2: Found regression line y = 0.1089*x + 0.8624
        }

        private static void Example3_Linear_regression_with_result_range_rescaled()
        {
            // This example is similar to example 2, but we want the results to be possibly in a bigger range, not only in 0..1.
            // To do that we need to scale the result. In this example we scale the results so that search area for solutions is between -1 million and +1 million.
            var scaleFactor = 1000000;
            var rescale = (double inputNumber) => inputNumber * scaleFactor * 2 - scaleFactor;

            var points = new Point[] {
                new Point{ x = 0.2, y = 33.6 },
                new Point{ x = 0.3, y = 24.7 },
                new Point{ x = 0.6, y = -71.8 },
                new Point{ x = 0.7, y = -73.9 },
            };
            var regressionLine = (double[] coefficients, Point point) => (rescale(coefficients[0]) * point.x) + rescale(coefficients[1]);
            var errorFunction = (double[] coefficients) => points.Sum(point => Math.Abs(regressionLine(coefficients, point) - point.y));
            var reverseErrorFunction = (double[] coefficients) => 1 / (errorFunction(coefficients) + 1);    // We need to reverse the function to maximize it rather than minimize it.
            var optimizer = new CompositeOptimizer();
            optimizer.FindMaximum(2, reverseErrorFunction, maxIterations: 100); // 2 means that we have 2 dimensions (i.e. 2 numbers in vector in the function)
            Console.WriteLine($"Example3: Found regression line y = {rescale(optimizer.BestSolutionSoFar[0]) :N4}*x + {rescale(optimizer.BestSolutionSoFar[1]) :N4}");
            // This prints:
            // Example3: Found regression line y= -218.4419*x + 78.9524
        }

        static void Main(string[] args)
        {
            Example1_FindMaximum();

            Example2_Linear_regression();

            Example3_Linear_regression_with_result_range_rescaled();

            Console.WriteLine("Press Enter.");
            Console.ReadLine();
        }
    }
}
