using DDTest.Battle;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Menu
{
	public class RosterCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[Inject]
		private readonly UnitConfig __unit;

		[Inject]
		private readonly UnitDrag __drag;

		[Inject]
		private readonly DragGhostView __ghost;


		[SerializeField, Required]
		private Image _portrait;

		[SerializeField, Required]
		private Image _selectedFrame;

		[SerializeField, Required]
		private Button _button;


		public UnitConfig Unit
			=> __unit;

		public Observable<Unit> Clicked
			=> _button.OnClickAsObservable();


		private void Awake()
		{
			_portrait.sprite = __unit.Sprite;
			SetSelected(false);
		}


		public void SetSelected(bool isSelected)
			=> _selectedFrame.enabled = isSelected;

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			__drag.Begin(__unit);
			__ghost.Show(__unit.Sprite, (RectTransform)transform, eventData);
		}

		public void OnDrag(PointerEventData eventData)
			=> __ghost.Follow(eventData);

		public void OnEndDrag(PointerEventData eventData)
		{
			__ghost.Hide();
			__drag.End();
		}


		public class Factory : PlaceholderFactory<UnitConfig, RosterCardView>
		{
		}
	}
}
