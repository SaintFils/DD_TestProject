using System;
using System.Collections.Generic;
using System.Linq;
using DDTest.Infrastructure;
using R3;

namespace DDTest.Battle
{
	public sealed class InitiativeQueue : IDisposable
	{
		private readonly List<BattleUnit> _tieBreakOrder;
		private readonly IRandom _random;
		private readonly bool _reshuffleEqualSpeedEachRound;
		private readonly ReactiveProperty<int> _round = new(0);
		private readonly ReactiveProperty<IReadOnlyList<BattleUnit>> _order = new(Array.Empty<BattleUnit>());
		private readonly ReactiveProperty<int> _currentIndex = new(-1);


		public ReadOnlyReactiveProperty<int> Round
			=> _round;

		public ReadOnlyReactiveProperty<IReadOnlyList<BattleUnit>> Order
			=> _order;

		public ReadOnlyReactiveProperty<int> CurrentIndex
			=> _currentIndex;


		public InitiativeQueue(IReadOnlyList<BattleUnit> units, IRandom random, bool reshuffleEqualSpeedEachRound)
		{
			_tieBreakOrder = units.ToList();
			_random = random;
			_reshuffleEqualSpeedEachRound = reshuffleEqualSpeedEachRound;
		}


		public void StartRound()
		{
			if (_round.Value == 0 || _reshuffleEqualSpeedEachRound)
				Shuffle(_tieBreakOrder);

			_currentIndex.Value = -1;
			_order.Value = _tieBreakOrder
				.Where(unit => unit.IsAlive)
				.OrderByDescending(unit => unit.Speed)
				.ToList();
			_round.Value++;
		}
		

		public void Dispose()
		{
			_round.Dispose();
			_order.Dispose();
			_currentIndex.Dispose();
		}

		private void Shuffle(List<BattleUnit> units)
		{
			for (int index = units.Count - 1; index > 0; index--)
			{
				int swapIndex = _random.Next(0, index + 1);
				(units[index], units[swapIndex]) = (units[swapIndex], units[index]);
			}
		}
	}
}
