using System.Collections.Generic;
using System.Linq;

namespace DDTest.Battle
{
	public sealed class BattleTeams
	{
		public IReadOnlyList<UnitConfig> Left { get; }

		public IReadOnlyList<UnitConfig> Right { get; }


		public BattleTeams(IReadOnlyList<UnitConfig> left, IReadOnlyList<UnitConfig> right)
		{
			Left = left.ToArray();
			Right = right.ToArray();
		}
	}
}
