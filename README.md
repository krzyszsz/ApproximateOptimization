# ApproximateOptimization
Heuristic optimizer using simulated annealing for problems whose solution is a vector of n-numbers in range <0, 1> and where we have a function returning a score (value) for each solution (the higher, the better). See [Wikipedia: Simulated annealing](https://en.wikipedia.org/wiki/Simulated_annealing)

On top of that this package contains "LocalAreaBinarySearch" (running for each problem dimension independently) working well only for certain types of problems and for small local areas of the function. Both these optimizers are composed into third type: "SimulatedAnnealingWithLocalAreaBinarySearch" which uses simulated annealing every second iteration and checks local neighberhood with binary search.
The final type "CompositeOptimizer" is additionally running the optimizers in multiple threads and returning the best result.

I'm aware there are a lot of other optimizers / solvers available but this one aims to be very simple and therefore easy to re-adjust to individual needs.

Example:

