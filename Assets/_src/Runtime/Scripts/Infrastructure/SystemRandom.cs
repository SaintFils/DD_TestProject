using System;

namespace DDTest.Infrastructure
{
	public sealed class SystemRandom : IRandom
	{
		private readonly Random _random;


		public int Seed { get; }


		public SystemRandom(int seed)
		{
			Seed = seed;
			_random = new Random(seed);
		}


		public int Next(int minInclusive, int maxExclusive)
			=> _random.Next(minInclusive, maxExclusive);

		public double NextDouble()
			=> _random.NextDouble();
	}
}
