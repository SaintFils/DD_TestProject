using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DDTest.Battle;

namespace DDTest.Tests
{
	public sealed class RecordingBattlePresenter : IBattlePresenter
	{
		public List<int> Rounds { get; } = new();

		public List<ActionResult> Results { get; } = new();

		public List<int> TargetHpOnStart { get; } = new();

		public List<int> TargetHpOnEnd { get; } = new();

		public List<BattleSideId?> Winners { get; } = new();


		public UniTask PlayRoundStartAsync(int round, CancellationToken cancellationToken)
		{
			Rounds.Add(round);
			return UniTask.CompletedTask;
		}

		public UniTask PlayActionStartAsync(ActionResult result, CancellationToken cancellationToken)
		{
			Results.Add(result);
			TargetHpOnStart.Add(GetTargetHp(result));
			return UniTask.CompletedTask;
		}

		public UniTask PlayActionEndAsync(ActionResult result, CancellationToken cancellationToken)
		{
			TargetHpOnEnd.Add(GetTargetHp(result));
			return UniTask.CompletedTask;
		}

		public UniTask PlayBattleEndAsync(BattleSideId? winner, CancellationToken cancellationToken)
		{
			Winners.Add(winner);
			return UniTask.CompletedTask;
		}

		private static int GetTargetHp(ActionResult result)
			=> result.Target != null ? result.Target.Hp.CurrentValue : -1;
	}
}
