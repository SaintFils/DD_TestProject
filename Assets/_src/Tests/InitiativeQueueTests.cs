using System.Linq;
using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class InitiativeQueueTests
	{
		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void StartRound_OrdersUnitsBySpeedDescending()
		{
			BattleConfig config = _configs.Battle(
				_configs.Team(_configs.Unit(speed: 3), _configs.Unit(speed: 7)),
				_configs.Team(_configs.Unit(speed: 5)));
			TestBattle battle = new(config);

			battle.State.Queue.StartRound();

			int[] speeds = battle.State.Queue.Order.CurrentValue.Select(unit => unit.Speed).ToArray();
			Assert.That(speeds, Is.EqualTo(new[] { 7, 5, 3 }));
		}

		[Test]
		public void StartRound_ExcludesDeadUnits()
		{
			BattleConfig config = _configs.Battle(
				_configs.Team(_configs.Unit(), _configs.Unit()),
				_configs.Team(_configs.Unit()));
			TestBattle battle = new(config);
			battle.Kill(battle.Left(1));

			battle.State.Queue.StartRound();

			Assert.That(battle.State.Queue.Order.CurrentValue, Has.No.Member(battle.Left(1)));
			Assert.That(battle.State.Queue.Order.CurrentValue.Count, Is.EqualTo(2));
		}

		[Test]
		public void StartRound_IncrementsRound()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit()));
			TestBattle battle = new(config);

			battle.State.Queue.StartRound();
			battle.State.Queue.StartRound();

			Assert.That(battle.State.Queue.Round.CurrentValue, Is.EqualTo(2));
		}

		[Test]
		public void StartRound_KeepsEqualSpeedOrderBetweenRounds_WhenReshuffleIsOff()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit()), reshuffleEqualSpeedEachRound: false);
			TestBattle battle = new(config);

			battle.State.Queue.StartRound();
			BattleUnit[] firstRound = battle.State.Queue.Order.CurrentValue.ToArray();
			battle.State.Queue.StartRound();

			Assert.That(battle.State.Queue.Order.CurrentValue, Is.EqualTo(firstRound));
		}

		[Test]
		public void StartRound_ReshufflesEqualSpeedOrderBetweenRounds_WhenReshuffleIsOn()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit()), reshuffleEqualSpeedEachRound: true);
			TestBattle battle = new(config);

			battle.State.Queue.StartRound();
			BattleUnit[] firstRound = battle.State.Queue.Order.CurrentValue.ToArray();
			battle.State.Queue.StartRound();

			Assert.That(battle.State.Queue.Order.CurrentValue, Is.EqualTo(firstRound.Reverse()));
		}

		[Test]
		public void TryMoveNext_SkipsUnitThatDiedThisRound()
		{
			BattleConfig config = _configs.Battle(
				_configs.Team(_configs.Unit(speed: 7)),
				_configs.Team(_configs.Unit(speed: 5), _configs.Unit(speed: 3)));
			TestBattle battle = new(config);
			InitiativeQueue queue = battle.State.Queue;
			queue.StartRound();

			queue.TryMoveNext(out BattleUnit first);
			battle.Kill(battle.Right(0));
			queue.TryMoveNext(out BattleUnit second);

			Assert.That(first, Is.SameAs(battle.Left(0)));
			Assert.That(second, Is.SameAs(battle.Right(1)));
		}

		[Test]
		public void TryMoveNext_ReturnsFalse_WhenRoundIsOver()
		{
			BattleConfig config = _configs.Battle(_configs.Team(_configs.Unit()), _configs.Team(_configs.Unit()));
			TestBattle battle = new(config);
			InitiativeQueue queue = battle.State.Queue;
			queue.StartRound();

			bool movedToFirst = queue.TryMoveNext(out _);
			bool movedToSecond = queue.TryMoveNext(out _);
			bool movedPastEnd = queue.TryMoveNext(out BattleUnit unit);

			Assert.That(movedToFirst && movedToSecond, Is.True);
			Assert.That(movedPastEnd, Is.False);
			Assert.That(unit, Is.Null);
			Assert.That(queue.CurrentIndex.CurrentValue, Is.EqualTo(queue.Order.CurrentValue.Count));
		}
	}
}
