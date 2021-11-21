# ApproximateOptimization
Heuristic optimizer using simulated annealing for problems whose solution is a vector of n-numbers in range <0, 1> and where we have a function returning a score (value) for each solution (the higher, the better). https://en.wikipedia.org/wiki/Simulated_annealing

Obviously a lot of problems can be adjusted to this optimizer.

I'm aware there are a lot of other optimizers / solvers available but this one aims to be very simple and therefore easy to re-adjust to the individual needs.

There are other "solutionFinders" in this project that are menat to be combined with simulated annealing.
** This is still work in progress: ** I want to add a composite optimizer that combines simulated annealing with local area binary search and runs it in multiple threads.
