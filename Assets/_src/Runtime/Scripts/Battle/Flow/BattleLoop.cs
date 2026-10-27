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


		public BattleLoop(BattleState battle, BattleRules rules, SideControllers controllers, IBattlePresenter presenter)
		{
			_battle = battle;
			_rules = rules;
			_controllers = controllers;
			_presenter = presenter;
		}


		public async UniTask<BattleSideId> RunAsync(CancellationToken cancellationToken)
		{
			InitiativeQueue queue = _battle.Queue;

			while (true)
			{
				queue.StartRound();
				await _presenter.PlayRoundStartAsync(queue.Round.CurrentValue, cancellationToken);

				while (queue.TryMoveNext(out BattleUnit actor))
				{
					await TakeTurnAsync(actor, cancellationToken);

					if (_battle.Winner == null)
						continue;

					BattleSideId winner = _battle.Winner.Value;
					await _presenter.PlayBattleEndAsync(winner, cancellationToken);

					return winner;
				}
			}
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
