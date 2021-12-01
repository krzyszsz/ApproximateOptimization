using NUnit.Framework;
using System;

namespace ApproximateOptimization.Tests
{
    public class SimulatedAnnealingOptimizerTests
    {
        private SimulatedAnnealingOptimizer<SimulatedAnnealingOptimizerParams> GetSut(Func<double[], double> func)
        {
            return new SimulatedAnnealingOptimizer<SimulatedAnnealingOptimizerParams>(
            new SimulatedAnnealingOptimizerParams
            {
                getValue = func,
                dimension = 2,
                maxIterations = 59,
            });
        }

        [Test]
        public void SaFindsGoodSolutionForLinearFunctionGrowingInBothDimentions()
        {
            Func<double[], double> func = (double[] vector) => vector[0] + vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(1).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(1).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(2).Within(0.01));
        }

        [Test]
        public void SaFindsGoodSolutionForLinearFuncDecreasingInBothDimensions()
        {
            Func<double[], double> func = (double[] vector) => -vector[0] - vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(0).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(0).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(0).Within(0.01));
        }

        [Test]
        public void SaFindsGoodSolutionForSinCos()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI)) + Math.Cos((vector[1] - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);
            double expectedX = 0.25;
            double expectedY = 0.4;
            double expectedBestValue = 2;

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
        }
    }
}