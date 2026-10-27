using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DDTest.Infrastructure;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public sealed class BattleEntryPoint : IInitializable, IDisposable
	{
		private readonly IRandom _random;
		private readonly BattlefieldView _battlefield;
		private readonly BattleLoop _loop;
		private readonly CancellationTokenSource _lifetime = new();


		public BattleEntryPoint(IRandom random, BattlefieldView battlefield, BattleLoop loop)
		{
			_random = random;
			_battlefield = battlefield;
			_loop = loop;
		}


		public void Initialize()
		{
			Debug.Log($"Battle, seed test: {_random.Seed}");

			_battlefield.Build();
			
			RunAsync(_lifetime.Token).Forget();
		}

		public void Dispose()
		{
			_lifetime.Cancel();
			_lifetime.Dispose();
		}

		private async UniTask RunAsync(CancellationToken cancellationToken)
		{
			BattleSideId winner = await _loop.RunAsync(cancellationToken);

			Debug.Log($"Battle finished, winner: {winner}");
		}
	}
}
