using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class InitiativeQueueView : MonoBehaviour
	{
		[Inject]
		private readonly BattleState __battle;


		[SerializeField]
		private QueueCardView[] _cards;


		private void Awake()
		{
			__battle.Queue.Order.Subscribe(Rebuild)
				.AddTo(this);

			__battle.Queue.CurrentIndex.Subscribe(_ => RefreshStates())
				.AddTo(this);
		}


		public UniTask PlayDeathAsync(BattleUnit unit, CancellationToken cancellationToken)
		{
			QueueCardView card = _cards.FirstOrDefault(candidate => candidate.Unit == unit);

			return card != null ? card.PlayDeathAsync(cancellationToken) : UniTask.CompletedTask;
		}

		private void Rebuild(IReadOnlyList<BattleUnit> order)
		{
			if (order.Count > _cards.Length)
				Debug.LogWarning($"InitiativeQueueView | {order.Count} units in the queue, only {_cards.Length} cards", this);

			for (int index = 0; index < _cards.Length; index++)
			{
				if (index < order.Count)
					_cards[index].Show(order[index]);
				else
					_cards[index].Hide();
			}

			RefreshStates();
		}

		private void RefreshStates()
		{
			IReadOnlyList<BattleUnit> order = __battle.Queue.Order.CurrentValue;
			int currentIndex = __battle.Queue.CurrentIndex.CurrentValue;
			int count = Mathf.Min(order.Count, _cards.Length);

			for (int index = 0; index < count; index++)
				_cards[index].SetState(index == currentIndex, index < currentIndex);
		}
	}
}
