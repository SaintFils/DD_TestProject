using DDTest.Infrastructure;

namespace DDTest.Tests
{
	public sealed class StubRandom : IRandom
	{
		private readonly double _nextDouble;


		public int Seed
			=> 0;


		public StubRandom(double nextDouble = 0)
		{
			_nextDouble = nextDouble;
		}


		public int Next(int minInclusive, int maxExclusive)
			=> minInclusive;

		public double NextDouble()
			=> _nextDouble;
	}
}
