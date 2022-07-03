using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingWithGradientAscentOptimizerParams : SimulatedAnnealingOptimizerParams
    {
        public double LocalAreaMultiplier { get; set; } = 0.4;
        public long GradientFollowingIterations { get; set; } = 20;
        public int JumpLengthIterationsFinal { get; set; } = 20;
        public int JumpLengthIterationsInitial { get; set; } = 6;
        public int FinalJumpsNumber { get; set; } = 6;

        public int SwitchingFreq { get; set; } = 200;

        public override void Validate()
        {
            base.Validate();
            if (LocalAreaMultiplier <= 0 || LocalAreaMultiplier >= 1)
            {
                throw new ArgumentException("LocalAreaMultiplier should be a number greater than 0 and less than 1.");
            }
        }

        public SimulatedAnnealingWithGradientAscentOptimizerParams ShallowClone()
        {
            return MemberwiseClone() as SimulatedAnnealingWithGradientAscentOptimizerParams;
        }
    }
}
