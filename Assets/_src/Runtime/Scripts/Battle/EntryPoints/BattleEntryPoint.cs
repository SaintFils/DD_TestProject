using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DDTest.Infrastructure;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public sealed class BattleEntryPoint : IInitializable
	{
		private readonly IRandom _random;
		private readonly BattleViewConfig _viewConfig;
		private readonly BattlefieldView _battlefield;
		private readonly BattleLogView _log;
		private readonly BattleLoop _loop;
		private readonly BattleSession _session;


		public BattleEntryPoint(IRandom random, BattleViewConfig viewConfig, BattlefieldView battlefield, BattleLogView log, BattleLoop loop, BattleSession session)
		{
			_random = random;
			_viewConfig = viewConfig;
			_battlefield = battlefield;
			_log = log;
			_loop = loop;
			_session = session;
		}


		public void Initialize()
		{
			Debug.Log($"Battle, seed test: {_random.Seed}");

			_battlefield.Build();
			_log.AddBattleStart(_random.Seed);

			RunAsync(_session.Token).Forget();
		}

		private async UniTask RunAsync(CancellationToken cancellationToken)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(_viewConfig.BattleStartDelay), cancellationToken: cancellationToken);

			BattleSideId? winner = await _loop.RunAsync(cancellationToken);

			Debug.Log(winner.HasValue ? $"Battle finished, winner: {winner.Value}" : "Battle finished in a draw");
		}
	}
}
