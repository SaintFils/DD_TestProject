using System.Threading;
using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class AiControllerTests
	{
		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void ChooseCommand_HealsMostWoundedAlly_WhenBelowThreshold()
		{
			AbilityConfig heal = _configs.Heal();
			AbilityConfig weakRanged = _configs.Ranged(power: 3);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(maxHp: 20), _configs.Unit(new[] { heal, weakRanged }), _configs.Unit(maxHp: 20)),
				_configs.Team(_configs.Unit())));
			battle.Left(0).TakeDamage(12);
			battle.Left(2).TakeDamage(14);

			BattleCommand command = ChooseCommand(battle, battle.Left(1));

			Assert.That(command.Ability, Is.SameAs(heal));
			Assert.That(command.Target, Is.SameAs(battle.Left(2)));
		}

		[Test]
		public void ChooseCommand_Attacks_WhenNoAllyIsBelowThreshold()
		{
			AbilityConfig heal = _configs.Heal();
			AbilityConfig weakRanged = _configs.Ranged(power: 3);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(new[] { heal, weakRanged })),
				_configs.Team(_configs.Unit())));

			BattleCommand command = ChooseCommand(battle, battle.Left(1));

			Assert.That(command.Ability, Is.SameAs(weakRanged));
			Assert.That(command.Target, Is.SameAs(battle.Right(0)));
		}

		[Test]
		public void ChooseCommand_PrefersAbilityWithHigherExpectedDamage()
		{
			AbilityConfig melee = _configs.Melee(power: 10);
			AbilityConfig ranged = _configs.Ranged(power: 4);
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { ranged, melee })), _configs.Team(_configs.Unit())));

			BattleCommand command = ChooseCommand(battle, battle.Left(0));

			Assert.That(command.Ability, Is.SameAs(melee));
		}

		[Test]
		public void ChooseCommand_TargetsEnemyWithLowerHp_WhenExpectedDamageIsEqual()
		{
			AbilityConfig ranged = _configs.Ranged();
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { ranged })), _configs.Team(_configs.Unit(), _configs.Unit())));
			battle.Right(1).TakeDamage(5);

			BattleCommand command = ChooseCommand(battle, battle.Left(0));

			Assert.That(command.Target, Is.SameAs(battle.Right(1)));
		}

		[Test]
		public void ChooseCommand_FinishesOffEnemy_WhenKillBonusOutweighsDamage()
		{
			AbilityConfig ranged = _configs.Ranged(power: 6);
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { ranged })), _configs.Team(_configs.Unit(), _configs.Unit())));
			battle.Right(1).TakeDamage(18);

			BattleCommand withoutBonus = ChooseCommand(battle, battle.Left(0));
			BattleCommand withBonus = ChooseCommand(battle, battle.Left(0), killBonus: 5f);

			Assert.That(withoutBonus.Target, Is.SameAs(battle.Right(0)));
			Assert.That(withBonus.Target, Is.SameAs(battle.Right(1)));
		}

		[Test]
		public void ChooseCommand_PicksRandomTarget_WithinTolerance()
		{
			AbilityConfig ranged = _configs.Ranged();
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(new[] { ranged })),
				_configs.Team(_configs.Unit(defense: 5), _configs.Unit(defense: 6), _configs.Unit(defense: 8))),
				new StubRandom(nextOffset: TeamConfig.SlotCount));

			BattleCommand strict = ChooseCommand(battle, battle.Left(0));
			BattleCommand tolerant = ChooseCommand(battle, battle.Left(0), targetTolerance: 0.1f);

			Assert.That(strict.Target, Is.SameAs(battle.Right(0)));
			Assert.That(tolerant.Target, Is.SameAs(battle.Right(1)));
		}

		[Test]
		public void ChooseCommand_Skips_WhenNoAbilityIsUsable()
		{
			AbilityConfig melee = _configs.Melee();
			UnitConfig fighter = _configs.Unit(new[] { melee });
			TestBattle battle = new(_configs.Battle(_configs.Team(fighter, fighter), _configs.Team(fighter)));

			BattleCommand command = ChooseCommand(battle, battle.Left(1));

			Assert.That(command.IsSkip, Is.True);
			Assert.That(command.Target, Is.Null);
		}

		[Test]
		public void ChooseCommand_UsesHealThresholdFromConfig()
		{
			AbilityConfig heal = _configs.Heal();
			AbilityConfig weakRanged = _configs.Ranged(power: 3);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(maxHp: 20), _configs.Unit(new[] { heal, weakRanged })),
				_configs.Team(_configs.Unit())));
			battle.Left(0).TakeDamage(12);

			BattleCommand cautious = ChooseCommand(battle, battle.Left(1), healThreshold: 0.5f);
			BattleCommand aggressive = ChooseCommand(battle, battle.Left(1), healThreshold: 0.3f);

			Assert.That(cautious.Ability, Is.SameAs(heal));
			Assert.That(aggressive.Ability, Is.SameAs(weakRanged));
		}

		[Test]
		public void ChooseCommand_PrefersAreaAttack_ThatHitsMoreEnemies()
		{
			AbilityConfig strike = _configs.Melee(power: 6);
			AbilityConfig sweep = _configs.Ability(AbilityTargetSide.Enemy, AbilityTargetFilter.Placement, AbilityEffect.Damage, 6, actorPosition: UnitPosition.Front, aoe: AbilityAoe.Behind);
			TestBattle battle = new(_configs.Battle(_configs.Team(_configs.Unit(new[] { strike, sweep })), _configs.Team(_configs.Unit(), _configs.Unit())));

			BattleCommand command = ChooseCommand(battle, battle.Left(0));

			Assert.That(command.Ability, Is.SameAs(sweep));
			Assert.That(command.Target, Is.SameAs(battle.Right(0)));
		}

		[Test]
		public void ChooseCommand_HealsNeighbours_WhenNeighbourIsWounded()
		{
			AbilityConfig healNeighbours = _configs.Ability(AbilityTargetSide.Self, AbilityTargetFilter.AnyLiving, AbilityEffect.Heal, 10, includeActor: false, aoe: AbilityAoe.Around);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(new[] { healNeighbours }), _configs.Unit()),
				_configs.Team(_configs.Unit())));
			battle.Left(0).TakeDamage(15);

			BattleCommand command = ChooseCommand(battle, battle.Left(1));

			Assert.That(command.Ability, Is.SameAs(healNeighbours));
			Assert.That(command.Target, Is.SameAs(battle.Left(1)));
		}

		[Test]
		public void ChooseCommand_DoesNotHealNeighbours_WhenOnlyHealerIsWounded()
		{
			AbilityConfig healNeighbours = _configs.Ability(AbilityTargetSide.Self, AbilityTargetFilter.AnyLiving, AbilityEffect.Heal, 10, includeActor: false, aoe: AbilityAoe.Around);
			AbilityConfig shot = _configs.Ranged(power: 3);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(new[] { healNeighbours, shot }), _configs.Unit()),
				_configs.Team(_configs.Unit())));
			battle.Left(1).TakeDamage(15);

			BattleCommand command = ChooseCommand(battle, battle.Left(1));

			Assert.That(command.Ability, Is.SameAs(shot));
		}

		[Test]
		public void ChooseCommand_HealsLowestHpRatio_EvenIfAnotherAllyMissesMoreHp()
		{
			AbilityConfig heal = _configs.Heal(power: 10);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(maxHp: 10), _configs.Unit(maxHp: 30), _configs.Unit(new[] { heal })),
				_configs.Team(_configs.Unit())));
			battle.Left(0).TakeDamage(7);
			battle.Left(1).TakeDamage(18);

			BattleCommand command = ChooseCommand(battle, battle.Left(2));

			Assert.That(command.Target, Is.SameAs(battle.Left(0)));
		}

		[Test]
		public void ChooseCommand_PrefersBiggerHeal_WhenLowestHpRatioIsEqual()
		{
			AbilityConfig heal = _configs.Heal(power: 10);
			AbilityConfig groupHeal = _configs.Ability(AbilityTargetSide.Ally, AbilityTargetFilter.AnyLiving, AbilityEffect.Heal, 10, aoe: AbilityAoe.WholeSide);
			TestBattle battle = new(_configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit(), _configs.Unit(new[] { heal, groupHeal })),
				_configs.Team(_configs.Unit())));
			battle.Left(0).TakeDamage(14);
			battle.Left(1).TakeDamage(12);

			BattleCommand command = ChooseCommand(battle, battle.Left(2));

			Assert.That(command.Ability, Is.SameAs(groupHeal));
		}

		private BattleCommand ChooseCommand(TestBattle battle, BattleUnit actor, float healThreshold = 0.5f, float killBonus = 0f, float targetTolerance = 0f)
			=> new AiController(battle.Rules, _configs.Ai(healThreshold, killBonus, targetTolerance), battle.Random)
				.ChooseCommandAsync(actor, CancellationToken.None)
				.GetAwaiter()
				.GetResult();
	}
}
