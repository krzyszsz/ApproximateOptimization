using NUnit.Framework;
using System;

namespace ApproximateOptimization.Tests
{
    public class AutoTuningTests
    {
        private ConcreteAutoTuningFinder GetSut(Func<double[], double> scoreFunc)
        {
            return OptimizerFactory.GetAutoSizingCompositeOptmizer(
                new SimulatedAnnealingWithLocalAreaBinarySearchParams
                {
                    getValue = scoreFunc,
                    dimension = 2,
                    maxIterations = 100,
                });
        }

        [Test]
        public void FindsGoodSolutionForLinearFunctionGrowingInBothDimentions()
        {
            Func<double[], double> func = (double[] vector) => vector[0] + vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.GreaterThan(115));
            Assert.That(sut.BestSolutionSoFar[1], Is.GreaterThan(115));
            Assert.That(sut.SolutionValue, Is.GreaterThan(115));
        }

        [Test]
        public void FindsGoodSolutionForLinearFuncDecreasingInBothDimensions()
        {
            Func<double[], double> func = (double[] vector) => -vector[0] - vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.LessThanOrEqualTo(-115));
            Assert.That(sut.BestSolutionSoFar[1], Is.LessThanOrEqualTo(-115));
            Assert.That(sut.SolutionValue, Is.GreaterThanOrEqualTo(+115.0*2));
        }

        [Test]
        public void FindsGoodSolutionForSinCos()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0]/100 * (2 * Math.PI)) + Math.Cos((vector[1]/123 - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);
            double expectedX = 0.25*100;
            double expectedY = 0.4*123;
            double expectedBestValue = 2;

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
        }

        public void FindsGoodSolutionForSinCosWidened()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI) / 5) + Math.Cos((vector[1] / 6 - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);
            double expectedX = 0.25 * 5;
            double expectedY = 0.4 * 6;
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