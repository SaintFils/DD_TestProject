using System;
using System.Collections.Generic;
using DDTest.Battle;
using DDTest.Infrastructure;
using R3;

namespace DDTest.Menu
{
	public sealed class TeamDraft
	{
		private readonly UnitConfig[] _slots = new UnitConfig[TeamConfig.SlotCount];
		private readonly Subject<Unit> _changed = new();


		public IReadOnlyList<UnitConfig> Slots
			=> _slots;

		public bool HasUnits
			=> Array.Exists(_slots, slot => slot != null);

		public bool HasEmptySlots
			=> Array.Exists(_slots, slot => slot == null);

		public Observable<Unit> Changed
			=> _changed;


		public bool TryAdd(UnitConfig unit)
		{
			int slotIndex = Array.FindIndex(_slots, slot => slot == null);

			if (slotIndex < 0)
				return false;

			_slots[slotIndex] = unit;
			_changed.OnNext(Unit.Default);

			return true;
		}

		public void Remove(int slotIndex)
		{
			if (_slots[slotIndex] == null)
				return;

			_slots[slotIndex] = null;
			_changed.OnNext(Unit.Default);
		}

		public void FillEmpty(IReadOnlyList<UnitConfig> roster, IRandom random)
		{
			if (roster.Count == 0 || !HasEmptySlots)
				return;

			for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
			{
				if (_slots[slotIndex] == null)
					_slots[slotIndex] = roster[random.Next(0, roster.Count)];
			}

			_changed.OnNext(Unit.Default);
		}
	}
}
