using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DDTest.Battle
{
	public sealed class BattlePresenter : IBattlePresenter
	{
		private readonly BattlefieldView _battlefield;


		public BattlePresenter(BattlefieldView battlefield)
		{
			_battlefield = battlefield;
		}


		public UniTask PlayRoundStartAsync(int round, CancellationToken cancellationToken)
		{
			Debug.Log($"Round {round}");

			return UniTask.CompletedTask;
		}

		public UniTask PlayActionStartAsync(ActionResult result, CancellationToken cancellationToken)
		{
			if (result.Target != null)
				_battlefield.GetView(result.Target).SetActionTarget(true);

			return UniTask.CompletedTask;
		}

		public UniTask PlayActionEndAsync(ActionResult result, CancellationToken cancellationToken)
		{
			Debug.Log(Describe(result));

			if (result.Target == null)
				return UniTask.CompletedTask;

			UnitView targetView = _battlefield.GetView(result.Target);
			targetView.SetActionTarget(false);

			if (!result.Target.IsAlive)
			{
				targetView.SetDead();
				Debug.Log($"{result.Target} dies");
			}

			return UniTask.CompletedTask;
		}

		private string Describe(ActionResult result)
			=> result.Outcome switch
			{
				ActionOutcome.Hit => $"{result.Actor} hits {result.Target} with {result.Ability.DisplayName} for {result.Amount} ({result.HitChance:P0})",
				ActionOutcome.Missed => $"{result.Actor} misses {result.Target} with {result.Ability.DisplayName} ({result.HitChance:P0})",
				ActionOutcome.Healed => $"{result.Actor} heals {result.Target} with {result.Ability.DisplayName} for {result.Amount}",
				_ => $"{result.Actor} uses {result.Ability.DisplayName}"
			};
	}
}
