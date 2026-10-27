using System.Threading;
using Cysharp.Threading.Tasks;

namespace DDTest.Battle
{
	public interface ISideController
	{
		UniTask<BattleCommand> ChooseCommandAsync(BattleUnit actor, CancellationToken cancellationToken);
	}
}
