using DG.Tweening;
using NaughtyAttributes;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class UnitView : MonoBehaviour
	{
		[Inject]
		private readonly BattleUnit __unit;

		[Inject]
		private readonly BattleViewConfig __viewConfig;


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

		[SerializeField, Range(1f, 1.5f)]
		private float _actionTargetScale = 1.1f;

		[SerializeField, Range(0f, 1f)]
		private float _deadAlpha = 0.3f;


		private float _targetFrameAlpha;
		private bool _isCurrentTurn;
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

		public void SetActionTarget(bool isActionTarget)
		{
			_isActionTarget = isActionTarget;
			transform.localScale = isActionTarget ? Vector3.one * _actionTargetScale : Vector3.one;
			RefreshHighlight();
		}

		public void SetTargetable(bool isTargetable)
			=> FadeTo(_targetFrame, isTargetable ? _targetFrameAlpha : 0f);

		public void SetDead()
			=> _canvasGroup.alpha = _deadAlpha;

		private void UpdateHp(int hp)
		{
			_hpText.text = $"{hp}/{__unit.MaxHp}";
			_hpFill.fillAmount = (float)hp / __unit.MaxHp;
		}

		private void RefreshHighlight()
		{
			Color color = _isActionTarget ? __viewConfig.SelectionColor : __viewConfig.CurrentTurnColor;
			bool isVisible = _isCurrentTurn || _isActionTarget;

			_highlightFrame.color = new Color(color.r, color.g, color.b, _highlightFrame.color.a);
			FadeTo(_highlightFrame, isVisible ? color.a : 0f);
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
