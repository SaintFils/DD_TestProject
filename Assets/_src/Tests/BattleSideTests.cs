using System;
using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class BattleSideTests
	{
		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void Frontline_IsFirstLivingUnitBySlot_WhenFirstSlotIsEmpty()
		{
			BattleConfig config = _configs.Battle(_configs.Team(null, _configs.Unit(), _configs.Unit()), _configs.Team(_configs.Unit()));
			TestBattle battle = new(config);

			Assert.That(battle.State.LeftSide.Frontline, Is.SameAs(battle.Left(1)));
		}

		[Test]
		public void Frontline_MovesToNextLivingUnit_WhenFrontlineDies()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit(), _configs.Unit(), _configs.Unit()), _configs.Team(_configs.Unit()));
			TestBattle battle = new(config);

			battle.Kill(battle.Left(0));

			Assert.That(battle.State.LeftSide.Frontline, Is.SameAs(battle.Left(1)));
		}

		[Test]
		public void Backline_MovesToPreviousLivingUnit_WhenBacklineDies()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit(), _configs.Unit(), _configs.Unit()), _configs.Team(_configs.Unit()));
			TestBattle battle = new(config);

			battle.Kill(battle.Left(2));

			Assert.That(battle.State.LeftSide.Backline, Is.SameAs(battle.Left(1)));
		}

		[Test]
		public void LastLivingUnit_IsBothFrontAndBack()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit(), _configs.Unit()), _configs.Team(_configs.Unit()));
			TestBattle battle = new(config);

			battle.Kill(battle.Left(0));

			Assert.That(battle.State.LeftSide.IsAt(battle.Left(1), UnitPosition.Front), Is.True);
			Assert.That(battle.State.LeftSide.IsAt(battle.Left(1), UnitPosition.Back), Is.True);
		}

		[Test]
		public void Constructor_Throws_WhenTeamHasNoUnits()
		{
			TeamConfig emptyTeam = _configs.Team();

			Assert.Throws<ArgumentException>(() => new BattleSide(BattleSideId.Left, emptyTeam));
		}
	}
}
