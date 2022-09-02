using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApproximateOptimization.Tests
{
    public class MultithreadedOptimizerTests
    {
        private MultithreadedOptimizer<SimulatedAnnealingOptimizerParams> GetSut(Func<double[], double> func, long maxIterations=70, CancellationToken cancellationToken=default(CancellationToken))
        {
            return new MultithreadedOptimizer<SimulatedAnnealingOptimizerParams>(
                new MultiThreadedOptimizerParams<SimulatedAnnealingOptimizerParams>
                {
                    ScoreFunction = func,
                    CreateOptimizer = (threadId) => new SimulatedAnnealingOptimizer(
                        new SimulatedAnnealingOptimizerParams
                        {
                            RandomSeed = threadId,
                            ScoreFunction = func,
                            Dimension = 2,
                            MaxIterations = maxIterations,
                            CancellationToken = cancellationToken
                        }),
                }
                );
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


        [Test]
        public void ReturnsCalculationStatistics()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI)) + Math.Cos((vector[1] - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);

            sut.FindMaximum();

            var stats = sut as IOptimizerStats;
            Assert.That(stats.IterationsExecuted, Is.GreaterThan(0));
            Assert.That(stats.LocalAreaAtTheEnd, Is.InRange(0.001, 0.5));
            Assert.That(stats.ElapsedTime.TotalSeconds, Is.GreaterThan(0.0));
        }

        [Test]
        public async Task StopsCalculationsOnRequest()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI)) + Math.Cos((vector[1] - 0.4) * (2 * Math.PI));
            var c = new CancellationTokenSource();
            var sut = GetSut(func, maxIterations: 100000000000, c.Token);

            _ = Task.Run(() => sut.FindMaximum());
            await Task.Delay(100);
            c.Cancel();
            await Task.Delay(1000);

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
        }
    }
}