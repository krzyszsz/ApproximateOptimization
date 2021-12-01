using System;

namespace ApproximateOptimization
{
    public class GradientAscentOptimizerParams : BaseOptimizerParams, IExternalOptimizerAware
    {
        public long iterationCount { get; set; } = 20;
        public long jumpLengthIterations { get; set; } = 20;
        public double MaxJump { get; set; } = 1.0;
        ExternallyInjectedOptimizerState IExternalOptimizerAware.externalOptimizerState { get; set; }

        public override void Validate()
        {
            base.Validate();
            if (MaxJump <= 0 || MaxJump > 1)
            {
                throw new ArgumentException("MaxJump should be a number greater than 0 and less than 1.");
            }
        }
    }
}
