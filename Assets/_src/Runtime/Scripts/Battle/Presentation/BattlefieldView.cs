using System;
using System.Collections.Generic;
using System.Linq;
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

		[Inject]
		private readonly PlayerController __player;


		[SerializeField]
		private RectTransform[] _leftSlots = new RectTransform[TeamConfig.SlotCount];

		[SerializeField]
		private RectTransform[] _rightSlots = new RectTransform[TeamConfig.SlotCount];


		private readonly Dictionary<BattleUnit, UnitView> _views = new();
		private UnitView _activeUnitView;


		public void Build()
		{
			Spawn(__battle.LeftSide, _leftSlots);
			Spawn(__battle.RightSide, _rightSlots);

			__battle.Queue.CurrentIndex.Subscribe(_ => RefreshActivity())
				.AddTo(this);

			__player.InspectedUnit.Subscribe(_ => RefreshActivity())
				.AddTo(this);

			__player.TargetableUnits.Subscribe(_ => RefreshTargetMarkers())
				.AddTo(this);

			__player.AffectedUnits.Subscribe(_ => RefreshTargetMarkers())
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

		private void RefreshActivity()
		{
			BattleUnit unit = __player.InspectedUnit.CurrentValue ?? __battle.Queue.Current;
			UnitView view = unit != null ? _views[unit] : null;

			if (view == _activeUnitView)
				return;

			if (_activeUnitView != null)
				_activeUnitView.SetActivity(false);

			_activeUnitView = view;

			if (_activeUnitView != null)
				_activeUnitView.SetActivity(true);
		}

		private void RefreshTargetMarkers()
		{
			IReadOnlyList<BattleUnit> affectedUnits = __player.AffectedUnits.CurrentValue;
			IReadOnlyList<BattleUnit> targetableUnits = affectedUnits.Count > 0 ? Array.Empty<BattleUnit>() : __player.TargetableUnits.CurrentValue;

			foreach (KeyValuePair<BattleUnit, UnitView> pair in _views)
			{
				pair.Value.SetTargetable(targetableUnits.Contains(pair.Key));
				pair.Value.SetInfluenced(affectedUnits.Contains(pair.Key));
			}
		}
	}
}
