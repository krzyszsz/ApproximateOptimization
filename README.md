# Approximate Optimization / Generic Problem Solver
This optimizer / solver can find solutions for problems where you can express the solution as an array of numbers and you can provide a function that tells the solver how good any particular array of numbers is. It rarely finds accurate solutions but for many problems they are "accurate enough".

# Techincal Details
As shown in the examples below, the easiest usage of the optimazer is via CompositeOptimizer which finds the solution using two alternating stages: simulated annealing is a probabilistic method scattering possible solutions across the whole multi-dimensional space and in the second stage a more deterministic method where the solution is narrowed down by binary search (to-do: gradient).

The implementation aims to limit memory allocations to improve performance and also runs multiple solvers in paralel threads.

Please note that when the range is not provided, only 0..1 is searched for all dimensions - see example 2. For automatic range discovery (slow!), see examples 3 and 4 employing AutoTuningFinder.

This is definitely not the most advanced solver you can find but (hopefully) it is simple and easy to customize & extend.

# Examples
```c#
// Note: There are more accurate and faster algorithms for the below problems which are problem-specific
// but these examples below demonstrate basic usage of the solver / optimizer and show that it can be useful
// as a tool to deal with much wider class of problems, as a more generic tool for data analysis.

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
    // Found regression line y = 0.1089*x + 0.8625
}

public static void Example3_Linear_regression_with_result_range_rescaled()
{
    // This example is similar to example 2, but it uses AutoTuningFinder
    // to automatically search wider range of numbers.
    var points = new Point[] {
        new Point{ x = 0.2, y = 33.6 },
        new Point{ x = 0.3, y = 24.7 },
        new Point{ x = 0.6, y = -71.8 },
        new Point{ x = 0.7, y = -73.9 },
    };
    var regressionLine = (double[] coefficients, Point point) =>
        (coefficients[0] * point.x) + coefficients[1];
    var errorFunction = (double[] coefficients) =>
        points.Sum(point => Math.Abs(regressionLine(coefficients, point) - point.y));
    // Below: We need to flip the sign of the function to minimize it rather than maximize it.
    var minusErrorFunc =
        (double[] coefficients) => -errorFunction(coefficients);
    var optimizer = new AutoTuningFinder(() => new CompositeOptimizer());
    optimizer.FindMaximum(2, minusErrorFunc, maxIterations: 100);
    Console.WriteLine(
        $"Found regression line " +
        $"y = {optimizer.BestSolutionSoFar[0]:N4}*x + {optimizer.BestSolutionSoFar[1]:N4}");
    // This prints:
    // Found regression line y = -120.5219*x + 10.4886
}

public static void Example4_Equation_solver()
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
    var optimizer = new AutoTuningFinder(() =>
        new CompositeOptimizer(iterationsPerDimension: 20, temperatureMultiplier: 0.99));
    optimizer.FindMaximum(2, minusErrorFunc, maxIterations: 5);
    Console.WriteLine(optimizer.SolutionFound && optimizer.SolutionValue < 0.1
        ? $"Equations' solution: x = {optimizer.BestSolutionSoFar[0]:N4} " +
        $"y = {optimizer.BestSolutionSoFar[1]:N4}"
        : "Solution not found.");
    // This prints:
    // Equations' solution: x = 0.6047 y = 0.3721
}
```
