using System;
using System.Linq;
using System.Threading;
using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class BattleLoopTests
	{
		private ConfigFactory _configs;
		private AbilityConfig _strike;


		[SetUp]
		public void SetUp()
		{
			_configs = new ConfigFactory();
			_strike = _configs.Melee(power: 100);
		}

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void RunAsync_ReturnsWinner_WhenOpposingSideIsDefeated()
		{
			TestBattle battle = CreateHeroAgainstTwoBandits();
			RecordingBattlePresenter presenter = new();

			BattleSideId winner = CreateLoop(battle, presenter).RunAsync(CancellationToken.None).GetAwaiter().GetResult();

			Assert.That(winner, Is.EqualTo(BattleSideId.Left));
			Assert.That(presenter.Rounds, Is.EqualTo(new[] { 1, 2 }));
		}

		[Test]
		public void RunAsync_PlaysBattleEndOnce_WithWinner()
		{
			TestBattle battle = CreateHeroAgainstTwoBandits();
			RecordingBattlePresenter presenter = new();

			CreateLoop(battle, presenter).RunAsync(CancellationToken.None).GetAwaiter().GetResult();

			Assert.That(presenter.Winners, Is.EqualTo(new[] { BattleSideId.Left }));
		}

		[Test]
		public void RunAsync_SkipsUnitThatDiedBeforeItsTurn()
		{
			TestBattle battle = CreateHeroAgainstTwoBandits();
			RecordingBattlePresenter presenter = new();

			CreateLoop(battle, presenter).RunAsync(CancellationToken.None).GetAwaiter().GetResult();

			Assert.That(presenter.Results.Select(result => result.Actor), Has.No.Member(battle.Right(0)));
		}

		[Test]
		public void RunAsync_AppliesResultBetweenActionStartAndEnd()
		{
			TestBattle battle = CreateHeroAgainstTwoBandits();
			RecordingBattlePresenter presenter = new();

			CreateLoop(battle, presenter).RunAsync(CancellationToken.None).GetAwaiter().GetResult();

			Assert.That(presenter.TargetHpOnStart[0], Is.EqualTo(20));
			Assert.That(presenter.TargetHpOnEnd[0], Is.EqualTo(0));
		}

		[Test]
		public void RunAsync_Throws_WhenCancelled()
		{
			TestBattle battle = CreateHeroAgainstTwoBandits();
			using CancellationTokenSource cancellation = new();
			cancellation.Cancel();
			BattleLoop loop = CreateLoop(battle, new RecordingBattlePresenter());

			Assert.That(() => loop.RunAsync(cancellation.Token).GetAwaiter().GetResult(), Throws.InstanceOf<OperationCanceledException>());
		}

		private TestBattle CreateHeroAgainstTwoBandits()
		{
			UnitConfig hero = _configs.Unit(new[] { _strike }, maxHp: 100, speed: 10);
			UnitConfig bandit = _configs.Unit(new[] { _configs.Melee(power: 1) }, maxHp: 20, speed: 1);

			return new TestBattle(_configs.Battle(_configs.Team(hero), _configs.Team(bandit, bandit)), new StubRandom(nextDouble: 0));
		}

		private BattleLoop CreateLoop(TestBattle battle, IBattlePresenter presenter)
		{
			ScriptedController hero = new(actor => new BattleCommand(actor, _strike, battle.State.RightSide.Frontline));

			return battle.CreateLoop(hero, new AiController(battle.Rules, _configs.Ai(), battle.Random), presenter);
		}
	}
}
