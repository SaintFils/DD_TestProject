using System;
using System.Linq;
using System.Threading;
using DDTest.Infrastructure;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public sealed class BattleEntryPoint : IInitializable, IDisposable
	{
		private const int DryRunRounds = 2;


		private readonly IRandom _random;
		private readonly BattleState _battle;
		private readonly CancellationTokenSource _lifetime = new();


		public BattleEntryPoint(IRandom random, BattleState battle)
		{
			_random = random;
			_battle = battle;
		}


		public void Initialize()
		{
			Debug.Log($"Battle, seed test: {_random.Seed}");
			Debug.Log($"Frontline: {_battle.LeftSide.Frontline} vs {_battle.RightSide.Frontline}");

			for (int round = 0; round < DryRunRounds; round++)
				LogNextRound();
		}

		public void Dispose()
		{
			_lifetime.Cancel();
			_lifetime.Dispose();
		}

		private void LogNextRound()
		{
			InitiativeQueue queue = _battle.Queue;
			queue.StartRound();

			string order = string.Join(" -> ", queue.Order.CurrentValue.Select(unit => $"{unit} speed {unit.Speed}"));
			Debug.Log($"Round {queue.Round.CurrentValue}: {order}");
		}
	}
}
