using System;
using DDTest.Infrastructure;

namespace DDTest.Tests
{
	public sealed class StubRandom : IRandom
	{
		private readonly double _nextDouble;
		private readonly int _nextOffset;


		public int Seed
			=> 0;


		public StubRandom(double nextDouble = 0, int nextOffset = 0)
		{
			_nextDouble = nextDouble;
			_nextOffset = nextOffset;
		}


		public int Next(int minInclusive, int maxExclusive)
			=> Math.Min(minInclusive + _nextOffset, maxExclusive - 1);

		public double NextDouble()
			=> _nextDouble;
	}
}
