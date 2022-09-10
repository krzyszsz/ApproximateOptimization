namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<MultiStrategyOptimizerParams>
    {
        public ConcreteCompositeParams(MultiStrategyOptimizerParams problemParameters)
        {
            ScoreFunction = problemParameters.ScoreFunction;
            CreateOptimizer = (long partitionId) =>
            {
                var newProblemParameters = problemParameters.ShallowClone();
                newProblemParameters.RandomSeed = (int)partitionId;
                return new MultiStrategyOptimizer(newProblemParameters);
            };
        }
    }
}
