using System;

namespace ApproximateOptimization
{
    public class GradientAscentOptimizerParams : BaseOptimizerParams, IExternalOptimizerAware
    {
        public int jumpLengthIterationsFinal { get; set; } = 20;
        public int jumpLengthIterationsInitial { get; set; } = 6;
        public int finalJumpsNumber { get; set; } = 6;
        public double MaxJump { get; set; } = 1.0;
        ExternallyInjectedOptimizerState IExternalOptimizerAware.externalOptimizerState { get; set; }

        public GradientAscentOptimizerParams()
        {
            maxIterations = maxIterations == 0 ? 20 : maxIterations;
        }

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
