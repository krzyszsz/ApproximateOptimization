using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingWithGradientAscentOptimizerParams : SimulatedAnnealingOptimizerParams
    {
        public double localAreaMultiplier { get; set; } = 0.4;
        public long gradientFollowingIterations { get; set; } = 20;
        public int jumpLengthIterationsFinal { get; set; } = 20;
        public int jumpLengthIterationsInitial { get; set; } = 6;
        public int finalJumpsNumber { get; set; } = 6;

        public override void Validate()
        {
            base.Validate();
            if (localAreaMultiplier <= 0 || localAreaMultiplier >= 1)
            {
                throw new ArgumentException("localAreaMultiplier multiplier should be a number greater than 0 and less than 1.");
            }
        }
    }
}
