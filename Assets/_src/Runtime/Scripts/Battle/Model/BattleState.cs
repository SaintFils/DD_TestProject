using System;
using System.Linq;
using DDTest.Infrastructure;

namespace DDTest.Battle
{
	public sealed class BattleState : IDisposable
	{
		public BattleSide LeftSide { get; }

		public BattleSide RightSide { get; }

		public InitiativeQueue Queue { get; }


		public BattleState(BattleConfig config, IRandom random)
		{
			LeftSide = new BattleSide(BattleSideId.Left, config.LeftTeam);
			RightSide = new BattleSide(BattleSideId.Right, config.RightTeam);
			Queue = new InitiativeQueue(LeftSide.Units.Concat(RightSide.Units).ToList(), random, config.ReshuffleEqualSpeedEachRound);
		}


		public void Dispose()
		{
			Queue.Dispose();
			LeftSide.Dispose();
			RightSide.Dispose();
		}
	}
}
