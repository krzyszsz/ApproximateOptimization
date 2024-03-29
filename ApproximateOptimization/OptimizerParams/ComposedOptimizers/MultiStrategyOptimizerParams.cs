﻿using System;

namespace ApproximateOptimization
{
    public class MultiStrategyOptimizerParams : SimulatedAnnealingOptimizerParams
    {
        public double LocalAreaMultiplier { get; set; } = 0.4;
        public long GradientFollowingIterations { get; set; } = 19;
        public int JumpLengthIterationsFinal { get; set; } = 19;
        public int JumpLengthIterationsInitial { get; set; } = 5;
        public int FinalJumpsNumber { get; set; } = 5;
        public long SwitchingFreq { get; set; } = 53;


        public bool GAEnabled { get; set; } = true;
        public int GAPeriod { get; set; } = 20; // Every 20 iterations
        public int GAPopulation { get; set; } = 4; // we get 4 best solutions to create cross-overs of them (this will generate 8 new iterations - each with random other x2)
        public int GAChildrenPerSolution { get; set; } = 2;

        public override void Validate()
        {
            base.Validate();
            if (LocalAreaMultiplier <= 0 || LocalAreaMultiplier >= 1)
            {
                throw new ArgumentException("LocalAreaMultiplier should be a number greater than 0 and less than 1.");
            }
        }

        public MultiStrategyOptimizerParams ShallowClone()
        {
            return MemberwiseClone() as MultiStrategyOptimizerParams;
        }
    }
}
