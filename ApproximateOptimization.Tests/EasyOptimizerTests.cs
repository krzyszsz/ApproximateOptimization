using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ApproximateOptimization.Tests
{
    public class EasyOptimizerTests
    {
        private IOptimizer GetSut(Func<double[], double> func, double requiredPrecision = 0.01)
        {
            return new EasyOptimizer(new EasyOptimizerParams
            {
                ScoreFunction = func,
                Dimension = 2,
                RequiredPrecision = requiredPrecision,
                Threads = 2,
            });
        }

        [Test]
        public void FindsGoodSolutionForLinearFunctionGrowingInBothDimentions()
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
        public void FindsGoodSolutionForLinearFuncDecreasingInBothDimensions()
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
        public void FindsGoodSolutionForSinCos()
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

        [TestCase(0.000001, 1)]
        public void FindsGoodSolutionAcrossWholeRange(double precision, int iterationsNumber)
        {
            Random random = new Random(0);
            for (int i = 0; i < 100; i++)
            {
                double expectedX = random.NextDouble();
                double expectedY = random.NextDouble();
                double expectedBestValue = random.NextDouble();
                Func<double[], double> func = (double[] vector) =>
                    -Math.Pow(vector[0] - expectedX, 2) - Math.Pow(vector[1] - expectedY, 2) + expectedBestValue;
                var sut = GetSut(func, iterationsNumber);

                sut.FindMaximum();

                Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
                Assert.That(sut.SolutionFound, Is.EqualTo(true));
                Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(precision));
                Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(precision));
                Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(precision));
            }
        }

        [Test]
        public void SolutionsAreNotRepeatable()
        {
            var solutions = new ConcurrentBag<double>();
            Func<double[], double> func = (double[] vector) =>
            {
                solutions.Add(vector[0]);
                return vector[0];
            };
            for (var i=0; i<100; i++)
            {
                var sut = new EasyOptimizer(new EasyOptimizerParams
                {
                    ScoreFunction = func,
                    Dimension = 1,
                    RequiredPrecision = 0.1,
                    Threads = 4,
                    NonRepeatableRandom = true,
                    MaxIterations = 1,
                    GAEnabled = false,
                    TabooSearch = false
                });
                sut.FindMaximum();
            }
            var histogram = solutions.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

            Assert.That(solutions.Count(), Is.EqualTo(100*4));
            Assert.That(histogram.Max(x => x.Value), Is.LessThan(5));
        }
    }

}