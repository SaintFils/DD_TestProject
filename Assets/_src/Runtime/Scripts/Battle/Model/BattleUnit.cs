using System;
using System.Collections.Generic;
using R3;

namespace DDTest.Battle
{
	public sealed class BattleUnit : IDisposable
	{
		private readonly ReactiveProperty<int> _hp;


		public UnitConfig Config { get; }

		public BattleSideId Side { get; }

		public int SlotIndex { get; }

		public ReadOnlyReactiveProperty<int> Hp
			=> _hp;

		public int MaxHp
			=> Config.MaxHp;

		public int Speed
			=> Config.Speed;

		public int Attack
			=> Config.Attack;

		public int Defense
			=> Config.Defense;

		public IReadOnlyList<AbilityConfig> Abilities
			=> Config.Abilities;

		public bool IsAlive
			=> _hp.Value > 0;


		public BattleUnit(UnitConfig config, BattleSideId side, int slotIndex)
		{
			Config = config;
			Side = side;
			SlotIndex = slotIndex;
			_hp = new ReactiveProperty<int>(config.MaxHp);
		}


		public void Dispose()
			=> _hp.Dispose();

		public override string ToString()
			=> $"{Config.DisplayName} ({Side} {SlotIndex})";
	}
}
