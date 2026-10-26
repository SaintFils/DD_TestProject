namespace DDTest.Infrastructure
{
	public interface IRandom
	{
		int Seed { get; }

		int Next(int minInclusive, int maxExclusive);

		double NextDouble();
	}
}
