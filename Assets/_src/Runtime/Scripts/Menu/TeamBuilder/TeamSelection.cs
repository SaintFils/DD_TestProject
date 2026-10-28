using System.Collections.Generic;
using DDTest.Battle;
using DDTest.Infrastructure;
using R3;

namespace DDTest.Menu
{
	public sealed class TeamSelection
	{
		private readonly TeamDraft _left = new();
		private readonly TeamDraft _right = new() { IsAi = true };


		public bool CanPlay
			=> _left.HasUnits && _right.HasUnits;

		public bool HasEmptySlots
			=> _left.HasEmptySlots || _right.HasEmptySlots;

		public Observable<Unit> Changed { get; }


		public TeamSelection()
		{
			Changed = _left.Changed.Merge(_right.Changed);
		}


		public TeamDraft Get(BattleSideId side)
			=> side == BattleSideId.Left ? _left : _right;

		public void FillEmpty(IReadOnlyList<UnitConfig> roster, IRandom random)
		{
			_left.FillEmpty(roster, random);
			_right.FillEmpty(roster, random);
		}

		public BattleTeams ToBattleTeams()
			=> new(_left.Slots, _right.Slots);
	}
}
