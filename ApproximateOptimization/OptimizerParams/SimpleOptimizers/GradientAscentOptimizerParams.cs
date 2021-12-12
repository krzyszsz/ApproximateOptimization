using System;

namespace ApproximateOptimization
{
    public class GradientAscentOptimizerParams : BaseOptimizerParams, IExternalOptimizerAware
    {
        public int JumpLengthIterationsFinal { get; set; } = 20;
        public int JumpLengthIterationsInitial { get; set; } = 6;
        public int FinalJumpsNumber { get; set; } = 6;
        public double MaxJump { get; set; } = 1.0;
        ExternallyInjectedOptimizerState IExternalOptimizerAware.ExternalOptimizerState { get; set; }

        public GradientAscentOptimizerParams()
        {
            MaxIterations = MaxIterations == 0 ? 20 : MaxIterations;
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
