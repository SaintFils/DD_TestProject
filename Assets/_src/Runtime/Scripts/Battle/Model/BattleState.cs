using System.Linq;
using DDTest.Infrastructure;

namespace DDTest.Battle
{
	public sealed class BattleState
	{
		private bool _isDraw;


		public BattleSide LeftSide { get; }

		public BattleSide RightSide { get; }

		public InitiativeQueue Queue { get; }

		public bool IsFinished
			=> _isDraw || !LeftSide.HasLivingUnits || !RightSide.HasLivingUnits;

		public BattleSideId? Winner
		{
			get
			{
				if (_isDraw || !IsFinished)
					return null;
				else
					return LeftSide.HasLivingUnits ? BattleSideId.Left : BattleSideId.Right;
			}
		}


		public BattleState(BattleConfig config, BattleTeams teams, IRandom random)
		{
			LeftSide = new BattleSide(BattleSideId.Left, teams.Left);
			RightSide = new BattleSide(BattleSideId.Right, teams.Right);
			Queue = new InitiativeQueue(LeftSide.Units.Concat(RightSide.Units).ToList(), random, config.ReshuffleEqualSpeedEachRound);
		}


		public BattleSide GetSide(BattleSideId id)
			=> id == BattleSideId.Left ? LeftSide : RightSide;

		public BattleSide GetOpposingSide(BattleSideId id)
			=> id == BattleSideId.Left ? RightSide : LeftSide;

		public void DeclareDraw()
			=> _isDraw = true;

		public void Apply(ActionResult result)
		{
			foreach (ActionEffect effect in result.Effects)
			{
				switch (effect.Outcome)
				{
					case ActionOutcome.Hit:
						effect.Target.TakeDamage(effect.Amount);
						break;

					case ActionOutcome.Healed:
						effect.Target.Heal(effect.Amount);
						break;
				}
			}
		}
	}
}
