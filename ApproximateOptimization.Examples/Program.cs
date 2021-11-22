﻿using ApproximateOptimization;

namespace ApproximateOptimizationExamples
{
    struct Point { public double x; public double y; }

    static class Program
    {
        public static void Example1_FindMaximum()
        {
            // Finds maximum of sin(x) * cos(y) for range x: 0..1 and y: 0..1
            var func = (double[] vector) => Math.Sin(vector[0]) * Math.Cos(vector[1]);
            var optimizer = new CompositeOptimizer();
            // Below: 2 means that we have 2 numbers in solution (dimension is 2)
            optimizer.FindMaximum(2, func, maxIterations: 100);
            Console.WriteLine(
                $"Maximum value {optimizer.SolutionValue} was found for " +
                $"x={optimizer.BestSolutionSoFar[0]} and y={optimizer.BestSolutionSoFar[1]} (x&y in 0..1).");
            // This prints:
            // Maximum value 0.8414709848078965 was found for x=1 and y=0 (x&y in 0..1).
        }

        public static void Example2_Linear_regression()
        {
            // Finds simple regression line for a couple of points. To do that we need to minimize the sum
            // of vertical distances to our line.
            // This obviously can use more complex regression models, not only lines
            // (imagine we could use trygonometric functions to make a slow version
            // of discrete frequency transformation).
            var points = new Point[] {
                new Point{ x = 13.2, y = 2.3 },
                new Point{ x = 23.3, y = 3.4 },
                new Point{ x = 33.6, y = 4.1 },
                new Point{ x = 34.7, y = 5.3 },
            };
            var regressionLine =
                (double[] coefficients, Point point) => (coefficients[0] * point.x) + coefficients[1];
            var errorFunction = (double[] coefficients) => points
                .Sum(point => Math.Abs(regressionLine(coefficients, point) - point.y));
            // Below: We need to flip the sign of the function to minimize it rather than maximize it.
            var minusErrorFunc = (double[] coefficients) => -errorFunction(coefficients);
            var optimizer = new CompositeOptimizer();
            optimizer.FindMaximum(2, minusErrorFunc, maxIterations: 100);
            Console.WriteLine(
                $"Found regression line " +
                $"y = {optimizer.BestSolutionSoFar[0]:N4}*x + {optimizer.BestSolutionSoFar[1]:N4}");
            // This prints:
            // Found regression line y = 0.1089*x + 0.8624
        }

        public static void Example3_Linear_regression_with_result_range_rescaled()
        {
            // This example is similar to example 2, but it looks for results in a wider range, not only in 0..1.
            // To do that we need to scale the result. Below search area is in range between -1 million and +1 million.
            var scaleFactor = 1000000;
            var rescale = (double inputNumber) => inputNumber * scaleFactor * 2 - scaleFactor;

            var points = new Point[] {
                new Point{ x = 0.2, y = 33.6 },
                new Point{ x = 0.3, y = 24.7 },
                new Point{ x = 0.6, y = -71.8 },
                new Point{ x = 0.7, y = -73.9 },
            };
            var regressionLine = (double[] coefficients, Point point) =>
                (rescale(coefficients[0]) * point.x) + rescale(coefficients[1]);
            var errorFunction = (double[] coefficients) =>
                points.Sum(point => Math.Abs(regressionLine(coefficients, point) - point.y));
            // Below: We need to flip the sign of the function to minimize it rather than maximize it.
            var minusErrorFunc =
                (double[] coefficients) => -errorFunction(coefficients);
            var optimizer = new CompositeOptimizer();
            optimizer.FindMaximum(2, minusErrorFunc, maxIterations: 100);
            Console.WriteLine(
                $"Found regression line " +
                $"y = {rescale(optimizer.BestSolutionSoFar[0]):N4}*x + {rescale(optimizer.BestSolutionSoFar[1]):N4}");
            // This prints:
            // Found regression line y = -218.4419*x + 78.9524
        }

        public static void Example4_Equation_solver()
        {
            // It only finds one solution even if there are more (in this example we have only one).
            // Say we have two equations:
            //  1:      x + 2 = 7 * y
            //  2:      y + 6*x = 4
            var scaleFactor = 100;
            var rescale = (double inputNumber) => inputNumber * scaleFactor * 2 - scaleFactor;

            var equations = new []
            {
                new Func<double[], double>[]
                {
                    (double[] variables) => rescale(variables[0]) + 2,  // left side, first equation
                    (double[] variables) => 7 * rescale(variables[1])   // right side, first equation
                },
                new Func<double[], double>[]
                {
                    (double[] variables) => rescale(variables[1]) + 6 * rescale(variables[0]),
                    (double[] variables) => 4 // right side, second equation
                },
            };

            var errorFunction = (double[] variables) => equations
                .Sum(sides => Math.Abs(sides[0](variables) - sides[1](variables)));
            // Below: We need to flip the sign of the error function to minimize it rather than maximize it.
            var minusErrorFunc = (double[] variables) => -errorFunction(variables);
            var optimizer = new CompositeOptimizer(iterationsPerDimension: 20, temperatureMultiplier: 0.99);
            optimizer.FindMaximum(2, minusErrorFunc, maxIterations: 5);
            Console.WriteLine(optimizer.SolutionFound && optimizer.SolutionValue < 0.1
                ? $"Equations' solution: x = {rescale(optimizer.BestSolutionSoFar[0]):N4} " +
                $"y = {rescale(optimizer.BestSolutionSoFar[1]):N4}"
                : "Solution not found.");
            // This prints:
            // Equations' solution: x = 0.6047 y = 0.3721
        }

        static void Main(string[] args)
        {
            Example1_FindMaximum();

            Example2_Linear_regression();

            Example3_Linear_regression_with_result_range_rescaled();

            Example4_Equation_solver();

            Console.WriteLine("Press Enter.");
            Console.ReadLine();
        }
    }
 }
