using System.Threading;
using Cysharp.Threading.Tasks;
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
		private Image _hpBackFill;

		[SerializeField, Required]
		private CanvasGroup _canvasGroup;


		private float _targetFrameAlpha;
		private Vector2 _bodyPosition;
		private bool _isCurrentTurn;
		private bool _isHoveredTarget;
		private bool _isActionTarget;


		public BattleUnit Unit
			=> __unit;

		public Vector3 BodyCenter
			=> _body.rectTransform.TransformPoint(_body.rectTransform.rect.center);

		private float Facing
			=> __unit.Side == BattleSideId.Left ? 1f : -1f;


		private void Awake()
		{
			_body.sprite = __unit.Config.Sprite;
			_nameText.text = __unit.Config.DisplayName;
			_targetFrameAlpha = _targetFrame.color.a;
			_bodyPosition = _body.rectTransform.anchoredPosition;

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

		public UniTask PlayLungeAsync(CancellationToken cancellationToken)
			=> PlayAsync(MoveBody(Facing * __viewConfig.LungeDistance, __viewConfig.LungeDuration), cancellationToken);

		public UniTask PlayLungeReturnAsync(CancellationToken cancellationToken)
			=> PlayAsync(MoveBody(0f, __viewConfig.LungeDuration), cancellationToken);

		public UniTask PlayRecoilAsync(CancellationToken cancellationToken)
			=> PlayAsync(SwayBody(-Facing * __viewConfig.RecoilDistance, __viewConfig.RecoilDuration), cancellationToken);

		public UniTask PlayCastAsync(CancellationToken cancellationToken)
			=> PlayAsync(_body.rectTransform.DOScale(__viewConfig.CastScale, __viewConfig.CastDuration / 2f).SetLoops(2, LoopType.Yoyo), cancellationToken);

		public UniTask PlayHitAsync(CancellationToken cancellationToken)
			=> UniTask.WhenAll(PlayAsync(_body.rectTransform.DOShakeAnchorPos(__viewConfig.HitDuration, __viewConfig.HitShakeStrength), cancellationToken),
				FlashAsync(__viewConfig.HitColor, __viewConfig.HitDuration, cancellationToken));

		public UniTask PlayDodgeAsync(CancellationToken cancellationToken)
			=> PlayAsync(SwayBody(-Facing * __viewConfig.DodgeDistance, __viewConfig.DodgeDuration), cancellationToken);

		public UniTask PlayHealAsync(CancellationToken cancellationToken)
			=> FlashAsync(__viewConfig.HealColor, __viewConfig.HealDuration, cancellationToken);

		public UniTask PlayDeathAsync(CancellationToken cancellationToken)
			=> PlayAsync(_canvasGroup.DOFade(__viewConfig.DeadAlpha, __viewConfig.DeathDuration), cancellationToken);

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
			float fillAmount = (float)hp / __unit.MaxHp;
			bool isRising = fillAmount > _hpFill.fillAmount;

			_hpText.text = $"{hp}/{__unit.MaxHp}";

			FillTo(isRising ? _hpBackFill : _hpFill, fillAmount, 0f, __viewConfig.HpFastDuration);
			FillTo(isRising ? _hpFill : _hpBackFill, fillAmount, __viewConfig.HpSlowDelay, __viewConfig.HpSlowDuration);
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

		private void FillTo(Image fill, float fillAmount, float delay, float duration)
		{
			fill.DOKill();
			fill.DOFillAmount(fillAmount, duration)
				.SetDelay(delay)
				.SetLink(gameObject);
		}

		private Tween MoveBody(float offset, float duration)
			=> _body.rectTransform.DOAnchorPosX(_bodyPosition.x + offset, duration);

		private Tween SwayBody(float offset, float duration)
			=> MoveBody(offset, duration / 2f).SetLoops(2, LoopType.Yoyo);

		private UniTask FlashAsync(Color color, float duration, CancellationToken cancellationToken)
			=> PlayAsync(_body.DOColor(color, duration / 2f).SetLoops(2, LoopType.Yoyo), cancellationToken);

		private UniTask PlayAsync(Tween tween, CancellationToken cancellationToken)
			=> tween.SetLink(gameObject).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);

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
