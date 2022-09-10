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
            int threadCounter = 0;
            var syncRoot = new object();
            CreateOptimizer = (long partitionId) =>
            {
                var newProblemParameters = problemParameters.ShallowClone();
                lock (syncRoot)
                {
                    if (threadCounter++ > 0)
                    {
                        newProblemParameters.StartSolution = null;
                        newProblemParameters.StartSolutionValue = null;
                    }
                }
                newProblemParameters.RandomSeed = (int)partitionId;
                return new MultiStrategyOptimizer(newProblemParameters);
            };
        }
    }
}
