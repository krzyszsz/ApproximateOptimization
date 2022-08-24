using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingOptimizerParams : BaseOptimizerParams
    {
        /// <summary>
        /// After every iteration the temperature used in the algorithm is multiplied by this value
        /// to narrow down the area of searches around the best solution found so far.
        /// </summary>
        public double TemperatureMultiplier { get; set; } = 0.9;

        /// <summary>
        /// Having initial temperature above 1.0 gives us a number of initial iterations
        /// where the whole solution area is taken into account.
        /// </summary>
        public double InitialTemperature { get; set; } = 2;

        /// <summary>
        /// Value used to initialize random numbers generator.
        /// </summary>
        public int RandomSeed { get; set; } = (int)(DateTime.UtcNow.Ticks % int.MaxValue);

        public int MaxStages { get; set; } = 4;

        public override void Validate()
        {
            base.Validate();
            if (TemperatureMultiplier <= 0 || TemperatureMultiplier >= 1)
            {
                throw new ArgumentException("TemperatureMultiplier should be a number greater than 0 and less than 1.");
            }

            if (MaxStages <= 0 || MaxStages > 10 || Math.Pow(MaxStages, Dimension) >= 1_000_000_000)
            {
                throw new ArgumentException("MaxStage should be a number from range 1..10 and MAX_STAGE to the power of DIMENSIONS should be less than 1_000_000_000.");
            }
        }
    }
}
