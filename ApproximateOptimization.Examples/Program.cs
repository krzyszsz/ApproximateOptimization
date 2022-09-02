using ApproximateOptimization;

namespace ApproximateOptimizationExamples
{
    struct Point { public double x; public double y; }

    static class Program
    {
        public static void Example1_FindMaximum()
        {
            // Finds maximum of sin(x) * cos(y) for range x: 0..1 and y: 0..1
            var func = (double[] vector) => Math.Sin(vector[0]) * Math.Cos(vector[1]);
            var optimizer = OptimizerFactory.GetCompositeOptimizer(
                new MultiStrategyOptimizerParams
                {
                    ScoreFunction = func,
                    Dimension = 2,
                    MaxIterations = 100,
                });
            optimizer.FindMaximum();
            Console.WriteLine(
                $"Maximum value {optimizer.SolutionValue} was found for " +
                $"x={optimizer.BestSolutionSoFar[0]:N4} and y={optimizer.BestSolutionSoFar[1]:N4} (x&y in 0..1).");
            // This prints:
            // Maximum value 0.8414709848078902 was found for x=1.0000 and y=0.0000 (x&y in 0..1).
        }

        public static void Example2_Linear_regression()
        {
            // This example finds a simple regression line for a couple of points.
            // To do that we need to minimize the sum of vertical distances to our line.
            // This obviously can use more complex regression models, not only lines.

            // Please note range discovery parameter. It's convenient but may be misleading;
            // it's better to give the expected range in the beginning because range discovery
            // is very simple and only widens the range when the found maximum is at the edge
            // of the range.
            // You may accidentally have a local maximum inside of the range and range
            // discovery will not trigger range widening, potentially missing global maximum).

            var points = new Point[] {
                new Point{ x = 1.32, y = 23 },
                new Point{ x = 2.33, y = 34 },
                new Point{ x = 3.36, y = 41 },
                new Point{ x = 3.47, y = 53 },
            };
            var regressionLine =
                (double[] coefficients, Point point) => (coefficients[0] * point.x) + coefficients[1];
            var errorFunction = (double[] coefficients) => points
                .Sum(point => Math.Abs(regressionLine(coefficients, point) - point.y));
            // Below: We need to flip the sign of the function to minimize it rather than maximize it.
            var minusErrorFunc = (double[] coefficients) => -errorFunction(coefficients);

            var optimizer = OptimizerFactory.GetCompositeOptimizer(
                new MultiStrategyOptimizerParams
                {
                    ScoreFunction = minusErrorFunc,
                    Dimension = 2,
                    MaxIterations = 100,
                }, rangeDiscovery: true); 
            optimizer.FindMaximum();
            Console.WriteLine(
                $"Found regression line " +
                $"y = {optimizer.BestSolutionSoFar[0]:N4}*x + {optimizer.BestSolutionSoFar[1]:N4}");
            // This prints:
            // Found regression line y = 10.8605*x + 8.6941
        }

        public static void Example3_Equation_solver()
        {
            // It only finds one solution even if there are more (in this example we have only one).
            // Say we have two equations:
            //  1:      x + 2 = 7 * y
            //  2:      y + 6*x = 4
            var equations = new []
            {
                new Func<double[], double>[]
                {
                    (double[] variables) => variables[0] + 2,  // left side, first equation
                    (double[] variables) => 7 * variables[1]   // right side, first equation
                },
                new Func<double[], double>[]
                {
                    (double[] variables) => variables[1] + 6 * variables[0],
                    (double[] variables) => 4 // right side, second equation
                },
            };

            var errorFunction = (double[] variables) => equations
                .Sum(sides => Math.Abs(sides[0](variables) - sides[1](variables)));
            // Below: We need to flip the sign of the error function to minimize it rather than maximize it.
            var minusErrorFunc = (double[] variables) => -errorFunction(variables);
            var optimizer = OptimizerFactory.GetCompositeOptimizer(
                new MultiStrategyOptimizerParams
                {
                    ScoreFunction = minusErrorFunc,
                    Dimension = 2,
                    MaxIterations = 100,
                    SolutionRange = new[] { new[] { -10.0, +10.0 }, new[] { -10.0, +10.0 } },
                }, rangeDiscovery: false);
            optimizer.FindMaximum();
            Console.WriteLine(optimizer.SolutionFound && optimizer.SolutionValue < 0.1
                ? $"Equations' solution: x = {optimizer.BestSolutionSoFar[0]:N4} " +
                $"y = {optimizer.BestSolutionSoFar[1]:N4}"
                : "Solution not found.");
            // This prints:
            // Equations' solution: x = 0.6043 y = 0.3725
        }

        public static void Example4_Easy_optimizer()
        {
			// This example demonstrates a simplified optimizer "EasyOptimizer"
			// which finds internal parameters automatically and only needs timeout and precision.
			// It resolves the same problem as example 1.
            var func = (double[] vector) => Math.Sin(vector[0]) * Math.Cos(vector[1]);
            var optimizer = OptimizerFactory.GetEasyOptimizer(
                new EasyOptimizerParams
                {
                    ScoreFunction = func,
                    Dimension = 2,
                    RequiredPrecision = 0.01
                });
            optimizer.FindMaximum();
            Console.WriteLine(
                $"Maximum value {optimizer.SolutionValue} was found for " +
                $"x={optimizer.BestSolutionSoFar[0]:N4} and y={optimizer.BestSolutionSoFar[1]:N4} (x&y in 0..1).");
            // This prints:
            // Maximum value 0.8414709848078904 was found for x=1.0000 and y=0.0000 (x&y in 0..1).
        }

        static void Main(string[] args)
        {
            Example1_FindMaximum();

            Example2_Linear_regression();

            Example3_Equation_solver();

            Example4_Easy_optimizer();

            Console.WriteLine("Press Enter.");
            Console.ReadLine();
        }
    }
 }
