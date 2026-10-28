using DDTest.Battle;
using R3;
using UnityEngine;
using Zenject;

namespace DDTest.Menu
{
	public class TeamSlotsView : MonoBehaviour
	{
		[Inject]
		private readonly TeamSelection __selection;


		[SerializeField]
		private BattleSideId _side;

		[SerializeField]
		private TeamSlotView[] _slots = new TeamSlotView[TeamConfig.SlotCount];


		private TeamDraft _draft;


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

			Refresh();
		}


		private void Refresh()
		{
			for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
				_slots[slotIndex].Show(_draft.Slots[slotIndex]);
		}
	}
}
