using System;
using System.Linq;
using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class BattleRulesTests
	{
		private const float Tolerance = 0.0001f;


		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void Melee_IsAvailableOnlyFromFrontline()
		{
			AbilityConfig melee = _configs.Melee();
			UnitConfig fighter = _configs.Unit(new[] { melee });
			TestBattle battle = new(_configs.Battle(_configs.Team(fighter, fighter), _configs.Team(fighter)));

			Assert.That(battle.Rules.CanUse(battle.Left(0), melee), Is.True);
			Assert.That(battle.Rules.CanUse(battle.Left(1), melee), Is.False);
		}

		[Test]
		public void Melee_BecomesAvailable_WhenUnitBecomesFrontline()
		{
			AbilityConfig melee = _configs.Melee();
			UnitConfig fighter = _configs.Unit(new[] { melee });
			TestBattle battle = new(_configs.Battle(_configs.Team(fighter, fighter), _configs.Team(fighter)));

			battle.Kill(battle.Left(0));

			Assert.That(battle.Rules.CanUse(battle.Left(1), melee), Is.True);
		}

		[Test]
		public void Melee_TargetsOnlyEnemyFrontline()
		{
			AbilityConfig melee = _configs.Melee();
			UnitConfig fighter = _configs.Unit(new[] { melee });
			TestBattle battle = new(_configs.Battle(_configs.Team(fighter), _configs.Team(fighter, fighter)));

			Assert.That(battle.Rules.GetTargets(battle.Left(0), melee), Is.EqualTo(new[] { battle.Right(0) }));

			battle.Kill(battle.Right(0));

			Assert.That(battle.Rules.GetTargets(battle.Left(0), melee), Is.EqualTo(new[] { battle.Right(1) }));
		}

		[Test]
		public void Ranged_TargetsAnyLivingEnemy()
		{
			AbilityConfig ranged = _configs.Ranged();
			UnitConfig archer = _configs.Unit(new[] { ranged });
			TestBattle battle = new(_configs.Battle(_configs.Team(archer), _configs.Team(archer, archer, archer)));

			battle.Kill(battle.Right(1));

			Assert.That(battle.Rules.GetTargets(battle.Left(0), ranged), Is.EqualTo(new[] { battle.Right(0), battle.Right(2) }));
		}

		[Test]
		public void HitChance_IsBase_WhenAttackEqualsDefense()
		{
			AbilityConfig ranged = _configs.Ranged();
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(new[] { ranged }, attack: 5)),
				_configs.Team(_configs.Unit(defense: 5))));

			float chance = battle.Rules.GetHitChance(battle.Left(1), battle.Right(0), ranged);

			Assert.That(chance, Is.EqualTo(battle.Config.BaseHitChance).Within(Tolerance));
		}

		[Test]
		public void HitChance_IsClampedToBounds()
		{
			AbilityConfig ranged = _configs.Ranged();
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(new[] { ranged }, attack: 50), _configs.Unit(new[] { ranged }, attack: 0)),
				_configs.Team(_configs.Unit(defense: 0), _configs.Unit(defense: 50))));

			Assert.That(battle.Rules.GetHitChance(battle.Left(1), battle.Right(0), ranged), Is.EqualTo(battle.Config.MaxHitChance).Within(Tolerance));
			Assert.That(battle.Rules.GetHitChance(battle.Left(2), battle.Right(1), ranged), Is.EqualTo(battle.Config.MinHitChance).Within(Tolerance));
		}

		[Test]
		public void HitChance_AppliesFrontlinePenalty_OnlyWhenActorIsFrontline()
		{
			AbilityConfig ranged = _configs.Ranged(frontlineAttackPenalty: 3);
			UnitConfig archer = _configs.Unit(new[] { ranged }, attack: 5);
			TestBattle battle = new(_configs.Battle(_configs.Team(archer, archer), _configs.Team(_configs.Unit(defense: 5))));
			float expectedPenalty = 3 * battle.Config.HitChancePerPoint;

			float frontlineChance = battle.Rules.GetHitChance(battle.Left(0), battle.Right(0), ranged);
			float backlineChance = battle.Rules.GetHitChance(battle.Left(1), battle.Right(0), ranged);

			Assert.That(backlineChance - frontlineChance, Is.EqualTo(expectedPenalty).Within(Tolerance));
		}

		[Test]
		public void Resolve_Misses_WhenRollIsNotBelowHitChance()
		{
			AbilityConfig melee = _configs.Melee();
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { melee })), _configs.Team(_configs.Unit())), new StubRandom(nextDouble: 0.99));

			ActionResult result = battle.Rules.Resolve(new BattleCommand(battle.Left(0), melee, battle.Right(0)));
			battle.State.Apply(result);

			Assert.That(result.Outcome, Is.EqualTo(ActionOutcome.Missed));
			Assert.That(battle.Right(0).Hp.CurrentValue, Is.EqualTo(battle.Right(0).MaxHp));
		}

		[Test]
		public void Resolve_Hits_WhenRollIsBelowHitChance()
		{
			AbilityConfig melee = _configs.Melee(power: 8);
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { melee })), _configs.Team(_configs.Unit(maxHp: 20))), new StubRandom(nextDouble: 0));

			ActionResult result = battle.Rules.Resolve(new BattleCommand(battle.Left(0), melee, battle.Right(0)));
			battle.State.Apply(result);

			Assert.That(result.Outcome, Is.EqualTo(ActionOutcome.Hit));
			Assert.That(battle.Right(0).Hp.CurrentValue, Is.EqualTo(12));
		}

		[Test]
		public void Heal_TargetsLivingAlliesIncludingSelf()
		{
			AbilityConfig heal = _configs.Heal();
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(new[] { heal }), _configs.Unit()),
				_configs.Team(_configs.Unit())));

			battle.Kill(battle.Left(2));

			Assert.That(battle.Rules.GetTargets(battle.Left(1), heal), Is.EqualTo(new[] { battle.Left(0), battle.Left(1) }));
		}

		[Test]
		public void Heal_IsCappedAtMaxHp_AndDoesNotRoll()
		{
			AbilityConfig heal = _configs.Heal(power: 10);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(maxHp: 20), _configs.Unit(new[] { heal })),
				_configs.Team(_configs.Unit())), new StubRandom(nextDouble: 0.99));
			battle.Left(0).TakeDamage(5);

			ActionResult result = battle.Rules.Resolve(new BattleCommand(battle.Left(1), heal, battle.Left(0)));
			battle.State.Apply(result);

			Assert.That(result.Outcome, Is.EqualTo(ActionOutcome.Healed));
			Assert.That(result.Amount, Is.EqualTo(5));
			Assert.That(battle.Left(0).Hp.CurrentValue, Is.EqualTo(20));
		}

		[Test]
		public void Skip_IsAvailableToEveryone_AndHasNoTargets()
		{
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(), _configs.Unit()), _configs.Team(_configs.Unit())));
			AbilityConfig skip = battle.Config.SkipAbility;

			ActionResult result = battle.Rules.Resolve(new BattleCommand(battle.Left(1), skip, null));

			Assert.That(battle.Rules.GetAbilities(battle.Left(1)), Has.Member(skip));
			Assert.That(battle.Rules.GetTargets(battle.Left(1), skip), Is.Empty);
			Assert.That(result.Outcome, Is.EqualTo(ActionOutcome.NoEffect));
		}

		[Test]
		public void Resolve_Throws_WhenTargetIsInvalid()
		{
			AbilityConfig melee = _configs.Melee();
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { melee })), _configs.Team(_configs.Unit(), _configs.Unit())));

			Assert.Throws<InvalidOperationException>(() => battle.Rules.Resolve(new BattleCommand(battle.Left(0), melee, battle.Right(1))));
		}

		[Test]
		public void Resolve_Throws_WhenUnitDoesNotHaveAbility()
		{
			AbilityConfig melee = _configs.Melee();
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit())));

			Assert.Throws<InvalidOperationException>(() => battle.Rules.Resolve(new BattleCommand(battle.Left(0), melee, battle.Right(0))));
		}

		[Test]
		public void GetAbilities_ReturnsUnitAbilitiesAndSkip()
		{
			AbilityConfig melee = _configs.Melee();
			AbilityConfig ranged = _configs.Ranged();
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { melee, ranged })), _configs.Team(_configs.Unit())));

			AbilityConfig[] abilities = battle.Rules.GetAbilities(battle.Left(0)).ToArray();

			Assert.That(abilities, Is.EqualTo(new[] { melee, ranged, battle.Config.SkipAbility }));
		}
	}
}
