using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DDTest.Battle;

namespace DDTest.Tests
{
	public sealed class ScriptedController : ISideController
	{
		private const int MaxCalls = 100;


		private readonly Func<BattleUnit, BattleCommand> _choose;
		private int _calls;


		public ScriptedController(Func<BattleUnit, BattleCommand> choose)
		{
			_choose = choose;
		}


		public UniTask<BattleCommand> ChooseCommandAsync(BattleUnit actor, CancellationToken cancellationToken)
		{
			if (++_calls > MaxCalls)
				throw new InvalidOperationException("Battle didn't finish in time");

			return UniTask.FromResult(_choose(actor));
		}
	}
}
