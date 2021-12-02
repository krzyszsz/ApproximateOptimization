# Approximate Optimization / Generic Problem Solver
A simple heuristic optimizer finding solution expressed as an array of numbers where a function can be provided telling the optimizer how good any particular array of numbers is. It rarely finds accurate solutions but for many problems they are "accurate enough".

# Technical Details
As shown in the examples below, the easiest usage of the optimizer is via CompositeOptimizer which finds the solution using two alternating stages: simulated annealing is a probabilistic method scattering possible solutions across the whole multi-dimensional space and in the second stage gradient ascent to systematically move towards the local maximum.

The implementation aims to limit memory allocations to improve performance and also runs multiple optimizers in parallel threads.

Please note that when the range is not provided, only 0..1 is searched for all dimensions - see example 1. For automatic range discovery (slow!), see example 2 employing rangeDiscovery. Also, by default the optimizer finds maximum, but you can flip the sign of the function to find minimum (examples 2 and 3).

This is definitely not the most advanced optimizer you can find but (hopefully) it is simple and easy to customize & extend.

# Examples
```c#
// Note: There are more accurate and faster algorithms for the below problems which are problem-specific
// but these examples below demonstrate basic usage of the solver / optimizer and show that it can be useful
// as a tool to deal with much wider class of problems, as a more generic tool for data analysis.

public static void Example1_FindMaximum()
{
    // Finds maximum of sin(x) * cos(y) for range x: 0..1 and y: 0..1
    var func = (double[] vector) => Math.Sin(vector[0]) * Math.Cos(vector[1]);
    var optimizer = OptimizerFactory.GetCompositeOptimizer(
        new SimulatedAnnealingWithGradientAscentOptimizerParams
        {
            scoreFunction = func,
            dimension = 2,
            maxIterations = 100,
        });
    optimizer.FindMaximum();
    Console.WriteLine(
        $"Maximum value {optimizer.SolutionValue} was found for " +
        $"x={optimizer.BestSolutionSoFar[0]:N4} and y={optimizer.BestSolutionSoFar[1]:N4} (x&y in 0..1).");
    // This prints:
    // Maximum value 0.8414708661335348 was found for x=1.0000 and y=0.0005 (x&y in 0..1).
}

public static void Example2_Linear_regression()
{
    // This example finds a simple regression line for a couple of points.
    // To do that we need to minimize the sum of vertical distances to our line.
    // This obviously can use more complex regression models, not only lines
    // (imagine we could use trigonometric functions to make a slow version
    // of discrete frequency transformation).

    // Please note range discovery parameter. It's convenient but may be misleading;
    // it's better to give the expected range in the beginning because range discovery
    // is very simple and only widens the range when the maximum is at the end of the range; 
    // you may accidentally have a local maximum inside of the range and range discovery
    // will not trigger range widening, potentially missing global maximum).

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
        new SimulatedAnnealingWithGradientAscentOptimizerParams
        {
            scoreFunction = minusErrorFunc,
            dimension = 2,
            maxIterations = 100,
        }, rangeDiscovery: true); 
    optimizer.FindMaximum();
    Console.WriteLine(
        $"Found regression line " +
        $"y = {optimizer.BestSolutionSoFar[0]:N4}*x + {optimizer.BestSolutionSoFar[1]:N4}");
    // This prints:
    // Found regression line y = 10.8911*x + 8.6238
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
        new SimulatedAnnealingWithGradientAscentOptimizerParams
        {
            scoreFunction = minusErrorFunc,
            dimension = 2,
            maxIterations = 100,
            solutionRange = new[] { new[] { -10.0, +10.0 }, new[] { -10.0, +10.0 } },
        }, rangeDiscovery: true);
    optimizer.FindMaximum();
    Console.WriteLine(optimizer.SolutionFound && optimizer.SolutionValue < 0.1
        ? $"Equations' solution: x = {optimizer.BestSolutionSoFar[0]:N4} " +
        $"y = {optimizer.BestSolutionSoFar[1]:N4}"
        : "Solution not found.");
    // This prints:
    // Equations' solution: x = 0.6046 y = 0.3721
}
```

# How accurate are the results? How long does it need to run?
It depends on the actual problem: if it converges to a single solution and you can use only gradient ascent optimizer, the result will be instant and very accurate. But for problems with multiple local maxima, the optimizer should start with a lot of iterations of simulated annealing. You can have a look at unit tests [HERE](https://github.com/krzyszsz/ApproximateOptimization/blob/master/ApproximateOptimization.Tests/CompositeOptimzerTests.cs#L85) and [HERE](https://github.com/krzyszsz/ApproximateOptimization/blob/master/ApproximateOptimization.Tests/GradientAscentOptimzerTests.cs#L66) and [HERE](https://github.com/krzyszsz/ApproximateOptimization/blob/master/ApproximateOptimization.Tests/SimulatedAnnealingOptimizerTests.cs#L73) in this project that demonstrate the precision of the results for various configurations. Obviously some problems don't have the properties necessary to use this tool (for example, this optimizer will not find the maximum of white noise easily because finding one high value of white noise does not imply that other high values are in the local area).

# Motivation
I have built this library for myself for some unspecified future projects around ML.
