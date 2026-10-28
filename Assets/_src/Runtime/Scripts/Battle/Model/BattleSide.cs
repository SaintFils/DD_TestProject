using System;
using System.Collections.Generic;

namespace DDTest.Battle
{
	public sealed class BattleSide
	{
		private readonly List<BattleUnit> _units = new();


		public BattleSideId Id { get; }

		public IReadOnlyList<BattleUnit> Units
			=> _units;

		public BattleUnit Frontline
			=> _units.Find(unit => unit.IsAlive);

		public BattleUnit Backline
			=> _units.FindLast(unit => unit.IsAlive);

		public bool HasLivingUnits
			=> _units.Exists(unit => unit.IsAlive);


		public BattleSide(BattleSideId id, IReadOnlyList<UnitConfig> slots)
		{
			Id = id;

			for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
			{
				UnitConfig unitConfig = slots[slotIndex];

				if (unitConfig != null)
					_units.Add(new BattleUnit(unitConfig, id, slotIndex));
			}

			if (_units.Count == 0)
				throw new ArgumentException($"The {id} side has no units", nameof(slots));
		}


		public BattleUnit GetUnitAt(UnitPosition position)
			=> position == UnitPosition.Front ? Frontline : Backline;

		public bool IsAt(BattleUnit unit, UnitPosition position)
			=> GetUnitAt(position) == unit;
	}
}
