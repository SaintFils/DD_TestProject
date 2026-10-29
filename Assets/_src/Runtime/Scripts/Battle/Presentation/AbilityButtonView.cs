using NaughtyAttributes;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class AbilityButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[Inject]
		private readonly PlayerController __player;


		[SerializeField, Required]
		private Button _button;

		[SerializeField, Required]
		private Image _icon;

		[SerializeField, Required]
		private TMP_Text _label;

		[SerializeField, Required]
		private Image _selectedFrame;


		public AbilityConfig Ability { get; private set; }


		private void Awake()
		{
			_button.OnClickAsObservable()
				.Subscribe(_ => __player.SelectAbility(Ability))
				.AddTo(this);
		}


		public void Show(AbilityConfig ability, bool isInteractable)
		{
			Ability = ability;
			_icon.sprite = ability.Icon;
			_icon.enabled = ability.Icon != null;
			_label.text = ability.DisplayName;
			_button.interactable = isInteractable;
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			Ability = null;
			gameObject.SetActive(false);
		}

		public void SetSelected(bool isSelected)
			=> _selectedFrame.enabled = isSelected;

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (Ability != null)
				__player.PreviewAbility(Ability);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (Ability != null)
				__player.ClearPreview(Ability);
		}
	}
}
