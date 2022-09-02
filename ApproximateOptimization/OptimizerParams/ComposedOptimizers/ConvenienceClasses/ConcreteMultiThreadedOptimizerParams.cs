namespace ApproximateOptimization
{
    /// <summary>
    /// Convenience class with all generic arguments provided.
    /// </summary>
    public class ConcreteMultiThreadedOptimizerParams : MultiThreadedOptimizerParams<MultiStrategyOptimizerParams>
    {
        public ConcreteMultiThreadedOptimizerParams(MultiStrategyOptimizerParams problemParameters)
        {
            ScoreFunction = problemParameters.ScoreFunction;
            CreateOptimizer = (int threadNumber) =>
            {
                var newProblemParameters = problemParameters.ShallowClone();
                newProblemParameters.RandomSeed = threadNumber;
                return new MultiStrategyOptimizer(newProblemParameters);
            };
        }
    }
}
