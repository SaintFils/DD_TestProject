using System;
using System.Threading;
using DDTest.Infrastructure;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public sealed class BattleEntryPoint : IInitializable, IDisposable
	{
		private readonly IRandom _random;
		private readonly CancellationTokenSource _lifetime = new();


		public BattleEntryPoint(IRandom random)
		{
			_random = random;
		}


		public void Initialize()
			=> Debug.Log($"Battle, seed test: {_random.Seed}");

		public void Dispose()
		{
			_lifetime.Cancel();
			_lifetime.Dispose();
		}
	}
}
