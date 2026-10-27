using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class BattleStateTests
	{
		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void Apply_Hit_DoesNotReduceHpBelowZero()
		{
			AbilityConfig melee = _configs.Melee(power: 50);
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit(new[] { melee })), _configs.Team(_configs.Unit(maxHp: 20), _configs.Unit()));
			TestBattle battle = new(config, new StubRandom(nextDouble: 0));

			ActionResult result = battle.Rules.Resolve(new BattleCommand(battle.Left(0), melee, battle.Right(0)));
			battle.State.Apply(result);

			Assert.That(result.Amount, Is.EqualTo(20));
			Assert.That(battle.Right(0).Hp.CurrentValue, Is.EqualTo(0));
			Assert.That(battle.Right(0).IsAlive, Is.False);
		}

		[Test]
		public void Battle_IsNotFinished_WhileBothSidesHaveLivingUnits()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit(), _configs.Unit()));
			TestBattle battle = new(config);

			battle.Kill(battle.Right(0));

			Assert.That(battle.State.IsFinished, Is.False);
			Assert.That(battle.State.Winner, Is.Null);
		}

		[Test]
		public void Battle_IsFinished_WhenOneSideHasNoLivingUnits()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit(), _configs.Unit()));
			TestBattle battle = new(config);

			battle.Kill(battle.Right(0));
			battle.Kill(battle.Right(1));

			Assert.That(battle.State.IsFinished, Is.True);
			Assert.That(battle.State.Winner, Is.EqualTo(BattleSideId.Left));
		}
	}
}
