using System;
using System.Threading;
using DDTest.Infrastructure;

namespace DDTest.Battle
{
	public sealed class BattleSession : IDisposable
	{
		private readonly SceneNavigator _navigator;
		private readonly CancellationTokenSource _lifetime = new();


		public CancellationToken Token
			=> _lifetime.Token;


		public BattleSession(SceneNavigator navigator)
		{
			_navigator = navigator;
		}


		public void Restart()
			=> Leave(_navigator.LoadBattle);

		public void ExitToMenu()
			=> Leave(_navigator.LoadMenu);

		public void Dispose()
		{
			_lifetime.Cancel();
			_lifetime.Dispose();
		}

		private void Leave(Action loadScene)
		{
			if (_lifetime.IsCancellationRequested)
				return;

			_lifetime.Cancel();
			loadScene();
		}
	}
}
