using System;
using DDTest.Battle;
using DDTest.Menu;
using NUnit.Framework;
using R3;

namespace DDTest.Tests
{
	public class TeamSelectionTests
	{
		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void TryAdd_FillsFirstEmptySlotFromFront_AfterRemovals()
		{
			TeamDraft draft = new();
			UnitConfig old = _configs.Unit();
			UnitConfig added = _configs.Unit();

			for (int slotIndex = 0; slotIndex < TeamConfig.SlotCount; slotIndex++)
				draft.TryAdd(old);

			draft.Remove(1);
			draft.Remove(3);
			draft.TryAdd(added);

			Assert.That(draft.Slots, Is.EqualTo(new[] { old, added, old, null }));

			draft.TryAdd(added);

			Assert.That(draft.Slots, Is.EqualTo(new[] { old, added, old, added }));
		}

		[Test]
		public void TryAdd_ReturnsFalse_WhenTeamIsFull()
		{
			TeamDraft draft = new();
			UnitConfig unit = _configs.Unit();

			for (int slotIndex = 0; slotIndex < TeamConfig.SlotCount; slotIndex++)
				draft.TryAdd(unit);

			Assert.That(draft.TryAdd(_configs.Unit()), Is.False);
			Assert.That(draft.Slots, Is.EqualTo(new[] { unit, unit, unit, unit }));
		}

		[Test]
		public void CanPlay_RequiresUnitsOnBothSides()
		{
			TeamSelection selection = new();
			UnitConfig unit = _configs.Unit();

			selection.Get(BattleSideId.Left).TryAdd(unit);

			Assert.That(selection.CanPlay, Is.False);

			selection.Get(BattleSideId.Right).TryAdd(unit);

			Assert.That(selection.CanPlay, Is.True);

			selection.Get(BattleSideId.Right).Remove(0);

			Assert.That(selection.CanPlay, Is.False);
		}

		[Test]
		public void FillEmpty_FillsOnlyEmptySlotsOnBothSides()
		{
			TeamSelection selection = new();
			UnitConfig kept = _configs.Unit();
			UnitConfig rolled = _configs.Unit();

			selection.Get(BattleSideId.Left).TryAdd(kept);
			selection.FillEmpty(new[] { kept, rolled }, new StubRandom(nextOffset: 1));

			Assert.That(selection.Get(BattleSideId.Left).Slots, Is.EqualTo(new[] { kept, rolled, rolled, rolled }));
			Assert.That(selection.Get(BattleSideId.Right).Slots, Is.EqualTo(new[] { rolled, rolled, rolled, rolled }));
			Assert.That(selection.HasEmptySlots, Is.False);
		}

		[Test]
		public void Changed_FiresOnlyWhenSlotsChange()
		{
			TeamSelection selection = new();
			TeamDraft left = selection.Get(BattleSideId.Left);
			UnitConfig unit = _configs.Unit();
			int changes = 0;

			using IDisposable subscription = selection.Changed.Subscribe(_ => changes++);

			left.TryAdd(unit);
			left.Remove(1);
			selection.Get(BattleSideId.Right).Remove(0);
			selection.FillEmpty(new[] { unit }, new StubRandom());
			selection.FillEmpty(new[] { unit }, new StubRandom());
			left.TryAdd(unit);

			Assert.That(changes, Is.EqualTo(3));
		}

		[Test]
		public void Controllers_DefaultToPlayerAgainstAi()
		{
			TeamSelection selection = new();

			Assert.That(selection.Get(BattleSideId.Left).IsAi, Is.False);
			Assert.That(selection.Get(BattleSideId.Right).IsAi, Is.True);
		}

		[Test]
		public void ToBattleTeams_ReturnsSlotsOfBothSides()
		{
			TeamSelection selection = new();
			UnitConfig leftUnit = _configs.Unit();
			UnitConfig rightUnit = _configs.Unit();

			selection.Get(BattleSideId.Left).TryAdd(leftUnit);
			selection.Get(BattleSideId.Right).TryAdd(rightUnit);
			BattleTeams teams = selection.ToBattleTeams();

			Assert.That(teams.Left, Is.EqualTo(new[] { leftUnit, null, null, null }));
			Assert.That(teams.Right, Is.EqualTo(new[] { rightUnit, null, null, null }));
		}
	}
}
