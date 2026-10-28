using DDTest.Battle;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Menu
{
	public class TeamSlotsView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[Inject]
		private readonly TeamSelection __selection;

		[Inject]
		private readonly UnitDrag __drag;


		[SerializeField]
		private BattleSideId _side;

		[SerializeField]
		private TeamSlotView[] _slots = new TeamSlotView[TeamConfig.SlotCount];

		[SerializeField, Required]
		private Image _highlightFrame;


		private TeamDraft _draft;
		private bool _isPointerOver;


		private void Awake()
		{
			_draft = __selection.Get(_side);

			for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
			{
				int removedSlotIndex = slotIndex;

				_slots[slotIndex].Clicked
					.Subscribe(_ => _draft.Remove(removedSlotIndex))
					.AddTo(this);
			}

			_draft.Changed
				.Subscribe(_ => Refresh())
				.AddTo(this);

			__drag.Unit
				.Subscribe(_ => RefreshHighlight())
				.AddTo(this);

			Refresh();
		}


		public void OnDrop(PointerEventData eventData)
		{
			UnitConfig unit = __drag.Unit.CurrentValue;

			if (unit != null)
				_draft.TryAdd(unit);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isPointerOver = true;
			RefreshHighlight();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isPointerOver = false;
			RefreshHighlight();
		}

		private void Refresh()
		{
			for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
				_slots[slotIndex].Show(_draft.Slots[slotIndex]);

			RefreshHighlight();
		}

		private void RefreshHighlight()
			=> _highlightFrame.enabled = _isPointerOver && __drag.Unit.CurrentValue != null && _draft.HasEmptySlots;
	}
}
