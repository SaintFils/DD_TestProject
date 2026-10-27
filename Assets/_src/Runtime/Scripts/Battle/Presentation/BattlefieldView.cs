using System.Collections.Generic;
using R3;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class BattlefieldView : MonoBehaviour
	{
		[Inject]
		private readonly BattleState __battle;

		[Inject]
		private readonly UnitView.Factory __unitViewFactory;


		[SerializeField]
		private RectTransform[] _leftSlots = new RectTransform[TeamConfig.SlotCount];

		[SerializeField]
		private RectTransform[] _rightSlots = new RectTransform[TeamConfig.SlotCount];


		private readonly Dictionary<BattleUnit, UnitView> _views = new();
		private UnitView _currentTurnView;


		public void Build()
		{
			Spawn(__battle.LeftSide, _leftSlots);
			Spawn(__battle.RightSide, _rightSlots);

			__battle.Queue.CurrentIndex
				.Subscribe(_ => HighlightCurrentTurn(__battle.Queue.Current))
				.AddTo(this);
		}

		public UnitView GetView(BattleUnit unit)
			=> _views[unit];

		private void Spawn(BattleSide side, IReadOnlyList<RectTransform> slots)
		{
			foreach (BattleUnit unit in side.Units)
			{
				UnitView view = __unitViewFactory.Create(unit);
				view.transform.SetParent(slots[unit.SlotIndex], false);
				_views.Add(unit, view);
			}
		}

		private void HighlightCurrentTurn(BattleUnit unit)
		{
			if (_currentTurnView != null)
				_currentTurnView.SetCurrentTurn(false);

			_currentTurnView = unit != null ? _views[unit] : null;

			if (_currentTurnView != null)
				_currentTurnView.SetCurrentTurn(true);
		}
	}
}
