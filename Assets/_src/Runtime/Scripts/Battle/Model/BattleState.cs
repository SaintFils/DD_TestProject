using System.Linq;
using DDTest.Infrastructure;

namespace DDTest.Battle
{
	public sealed class BattleState
	{
		public BattleSide LeftSide { get; }

		public BattleSide RightSide { get; }

		public InitiativeQueue Queue { get; }

		public bool IsFinished
			=> !LeftSide.HasLivingUnits || !RightSide.HasLivingUnits;

		public BattleSideId? Winner
		{
			get
			{
				if (!IsFinished)
					return null;
				else
					return LeftSide.HasLivingUnits ? BattleSideId.Left : BattleSideId.Right;
			}
		}


		public BattleState(BattleConfig config, IRandom random)
		{
			LeftSide = new BattleSide(BattleSideId.Left, config.LeftTeam);
			RightSide = new BattleSide(BattleSideId.Right, config.RightTeam);
			Queue = new InitiativeQueue(LeftSide.Units.Concat(RightSide.Units).ToList(), random, config.ReshuffleEqualSpeedEachRound);
		}


		public BattleSide GetSide(BattleSideId id)
			=> id == BattleSideId.Left ? LeftSide : RightSide;

		public BattleSide GetOpposingSide(BattleSideId id)
			=> id == BattleSideId.Left ? RightSide : LeftSide;

		public void Apply(ActionResult result)
		{
			switch (result.Outcome)
			{
				case ActionOutcome.Hit:
					result.Target.TakeDamage(result.Amount);
					break;

				case ActionOutcome.Healed:
					result.Target.Heal(result.Amount);
					break;
			}
		}
	}
}
