using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace DDTest.Battle
{
	public sealed class PlayerController : ISideController, IDisposable
	{
		private readonly ReactiveProperty<BattleUnit> _awaitingActor = new();
		private UniTaskCompletionSource<BattleCommand> _commandSource;


		public ReadOnlyReactiveProperty<BattleUnit> AwaitingActor
			=> _awaitingActor;


		public async UniTask<BattleCommand> ChooseCommandAsync(BattleUnit actor, CancellationToken cancellationToken)
		{
			_commandSource = new UniTaskCompletionSource<BattleCommand>();
			_awaitingActor.Value = actor;

			BattleCommand command = await _commandSource.Task.AttachExternalCancellation(cancellationToken);

			_awaitingActor.Value = null;
			_commandSource = null;

			return command;
		}

		public void Dispose()
			=> _awaitingActor.Dispose();
	}
}
