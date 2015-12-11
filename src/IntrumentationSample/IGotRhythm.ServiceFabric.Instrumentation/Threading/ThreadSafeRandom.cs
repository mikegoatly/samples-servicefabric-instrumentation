using System;
using System.Threading;

namespace IGotRhythm.ServiceFabric.Instrumentation.Threading
{
    public static class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();
        private static readonly ThreadLocal<Random> _local = new ThreadLocal<Random>(GetLocalRandom);

        private static Random GetLocalRandom()
        {
            int seed;
            lock (_global)
            {
                seed = _global.Next();
            }
            return new Random(seed);
        }

        public static Random Value => _local.Value;
    }
}