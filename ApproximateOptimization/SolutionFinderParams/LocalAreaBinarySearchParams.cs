using System;

namespace ApproximateOptimization
{
    public class LocalAreaBinarySearchParams : BaseSolutionFinderParams, IExternalOptimazerAware
    {
        public double localArea { get; set; } = 1.0;
        public long maxBinarySearchIterations { get; set; } = 3;
        public long iterationsPerDimension { get; set; } = 10;
        ExternallyInjectedOptimizerState IExternalOptimazerAware.externalOptimizerState { get; set; }

        public override void ValidateArguments()
        {
            base.ValidateArguments();
            if (localArea <= 0 || localArea > 1)
            {
                throw new ArgumentException("LocalArea should be a number greater than 0 and less or equal 1.");
            }
        }
    }
}
