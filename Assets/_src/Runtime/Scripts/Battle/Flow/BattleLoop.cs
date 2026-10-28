using System.Threading;
using Cysharp.Threading.Tasks;

namespace DDTest.Battle
{
	public sealed class BattleLoop
	{
		private readonly BattleState _battle;
		private readonly BattleRules _rules;
		private readonly SideControllers _controllers;
		private readonly IBattlePresenter _presenter;
		private readonly int _roundLimit;


		public BattleLoop(BattleState battle, BattleRules rules, SideControllers controllers, IBattlePresenter presenter, int roundLimit)
		{
			_battle = battle;
			_rules = rules;
			_controllers = controllers;
			_presenter = presenter;
			_roundLimit = roundLimit;
		}


		public async UniTask<BattleSideId?> RunAsync(CancellationToken cancellationToken)
		{
			InitiativeQueue queue = _battle.Queue;

			while (queue.Round.CurrentValue < _roundLimit)
			{
				queue.StartRound();
				await _presenter.PlayRoundStartAsync(queue.Round.CurrentValue, cancellationToken);

				while (queue.TryMoveNext(out BattleUnit actor))
				{
					await TakeTurnAsync(actor, cancellationToken);

					if (_battle.IsFinished)
						return await EndAsync(cancellationToken);
				}
			}

			_battle.DeclareDraw();

			return await EndAsync(cancellationToken);
		}

		private async UniTask<BattleSideId?> EndAsync(CancellationToken cancellationToken)
		{
			BattleSideId? winner = _battle.Winner;

			await _presenter.PlayBattleEndAsync(winner, cancellationToken);

			return winner;
		}

		private async UniTask TakeTurnAsync(BattleUnit actor, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			BattleCommand command = await _controllers.Get(actor.Side).ChooseCommandAsync(actor, cancellationToken);
			ActionResult result = _rules.Resolve(command);

			await _presenter.PlayActionStartAsync(result, cancellationToken);
			
			_battle.Apply(result);
			
			await _presenter.PlayActionEndAsync(result, cancellationToken);
		}
	}
}
