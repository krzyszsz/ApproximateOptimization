﻿using System;

namespace ApproximateOptimization
{
    public class AutoTuningParams<T> : BaseSolutionFinderParams where T : BaseSolutionFinderParams
    {
        public Func<ISolutionFinder<T>> solutionFinderFactoryMethod { get; set; }
        public int maxAttempts { get; set; } = 50;
    }
}