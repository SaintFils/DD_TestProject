using System;
using System.Collections.Generic;
using System.Linq;

namespace DDTest.Battle
{
	public sealed class BattleSide : IDisposable
	{
		private readonly List<BattleUnit> _units = new();


		public BattleSideId Id { get; }

		public IReadOnlyList<BattleUnit> Units
			=> _units;

		public BattleUnit Frontline
			=> _units.FirstOrDefault(unit => unit.IsAlive);


		public BattleSide(BattleSideId id, TeamConfig team)
		{
			Id = id;

			for (int slotIndex = 0; slotIndex < team.Slots.Count; slotIndex++)
			{
				UnitConfig unitConfig = team.Slots[slotIndex];

				if (unitConfig != null)
					_units.Add(new BattleUnit(unitConfig, id, slotIndex));
			}
		}


		public void Dispose()
		{
			foreach (BattleUnit unit in _units)
				unit.Dispose();
		}
	}
}
