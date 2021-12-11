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
        public int RandomSeed { get; set; } = 0;

        public override void Validate()
        {
            base.Validate();
            if (TemperatureMultiplier <= 0 || TemperatureMultiplier >= 1)
            {
                throw new ArgumentException("TemperatureMultiplier should be a number greater than 0 and less than 1.");
            }
        }
    }
}
