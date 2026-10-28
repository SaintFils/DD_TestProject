using System.Threading;
using Cysharp.Threading.Tasks;

namespace DDTest.Battle
{
	public interface IBattlePresenter
	{
		UniTask PlayRoundStartAsync(int round, CancellationToken cancellationToken);

		UniTask PlayActionStartAsync(ActionResult result, CancellationToken cancellationToken);

		UniTask PlayActionEndAsync(ActionResult result, CancellationToken cancellationToken);

		UniTask PlayBattleEndAsync(BattleSideId? winner, CancellationToken cancellationToken);
	}
}
