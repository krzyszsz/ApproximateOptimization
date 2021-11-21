using System;

namespace ApproximateOptimization
{
    public class ThreadSafeConsoleLogger : ILogger
    {
        private object syncObject = new object();

        private ThreadSafeConsoleLogger()
        {}

        public static ILogger Instance { get; } = new ThreadSafeConsoleLogger();

        public void Error(string message)
        {
            lock(syncObject)
            {
                Console.WriteLine(message);
            }
        }

        public void Info(string message)
        {
            lock (syncObject)
            {
                Console.WriteLine(message);
            }
        }
    }
}
