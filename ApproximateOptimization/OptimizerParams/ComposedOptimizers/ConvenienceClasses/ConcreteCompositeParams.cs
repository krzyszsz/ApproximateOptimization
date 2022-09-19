using System;

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
                newProblemParameters.RandomSeed = (problemParameters.NonRepeatableRandom ? (int)(DateTime.UtcNow.Ticks + partitionId * 1000_000_000L) : (int)partitionId);
                return new MultiStrategyOptimizer(newProblemParameters);
            };
        }
    }
}
