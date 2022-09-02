namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<MultiStrategyOptimizerParams>
    {
        public ConcreteCompositeParams(MultiStrategyOptimizerParams problemParameters)
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
