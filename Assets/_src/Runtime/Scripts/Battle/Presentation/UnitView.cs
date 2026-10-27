using DG.Tweening;
using NaughtyAttributes;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class UnitView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[Inject]
		private readonly BattleUnit __unit;

		[Inject]
		private readonly BattleViewConfig __viewConfig;

		[Inject]
		private readonly PlayerController __player;


		[SerializeField, Required]
		private Image _body;

		[SerializeField, Required]
		private Image _highlightFrame;

		[SerializeField, Required]
		private Image _targetFrame;

		[SerializeField, Required]
		private TMP_Text _nameText;

		[SerializeField, Required]
		private TMP_Text _hpText;

		[SerializeField, Required]
		private Image _hpFill;

		[SerializeField, Required]
		private CanvasGroup _canvasGroup;


		private float _targetFrameAlpha;
		private bool _isCurrentTurn;
		private bool _isHoveredTarget;
		private bool _isActionTarget;


		public BattleUnit Unit
			=> __unit;


		private void Awake()
		{
			_body.sprite = __unit.Config.Sprite;
			_nameText.text = __unit.Config.DisplayName;
			_targetFrameAlpha = _targetFrame.color.a;

			SetAlpha(_targetFrame, 0f);
			SetAlpha(_highlightFrame, 0f);

			__unit.Hp.Subscribe(UpdateHp)
				.AddTo(this);
		}


		public void SetCurrentTurn(bool isCurrentTurn)
		{
			_isCurrentTurn = isCurrentTurn;
			RefreshHighlight();
		}

		public void SetHoveredTarget(bool isHoveredTarget)
		{
			_isHoveredTarget = isHoveredTarget;
			RefreshHighlight();
		}

		public void SetActionTarget(bool isActionTarget)
		{
			_isActionTarget = isActionTarget;
			transform.localScale = isActionTarget ? Vector3.one * __viewConfig.ActionTargetScale : Vector3.one;
			RefreshHighlight();
		}

		public void SetTargetable(bool isTargetable)
			=> FadeTo(_targetFrame, isTargetable ? _targetFrameAlpha : 0f);

		public void SetDead()
			=> _canvasGroup.alpha = __viewConfig.DeadAlpha;

		public void OnPointerEnter(PointerEventData eventData)
			=> __player.HoverTarget(__unit);

		public void OnPointerExit(PointerEventData eventData)
			=> __player.UnhoverTarget(__unit);

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				__player.SelectTarget(__unit);
		}

		private void UpdateHp(int hp)
		{
			_hpText.text = $"{hp}/{__unit.MaxHp}";
			_hpFill.fillAmount = (float)hp / __unit.MaxHp;
		}

		private void RefreshHighlight()
		{
			bool isSelected = _isHoveredTarget || _isActionTarget;

			if (!_isCurrentTurn && !isSelected)
			{
				FadeTo(_highlightFrame, 0f);

				return;
			}

			Color color = isSelected ? __viewConfig.SelectionColor : __viewConfig.CurrentTurnColor;

			_highlightFrame.color = new Color(color.r, color.g, color.b, _highlightFrame.color.a);
			FadeTo(_highlightFrame, color.a);
		}

		private void FadeTo(Image image, float alpha)
		{
			image.DOKill();
			image.DOFade(alpha, __viewConfig.HighlightFadeDuration)
				.SetLink(gameObject);
		}

		private static void SetAlpha(Image image, float alpha)
		{
			Color color = image.color;
			color.a = alpha;
			image.color = color;
		}


		public class Factory : PlaceholderFactory<BattleUnit, UnitView>
		{
		}
	}
}
