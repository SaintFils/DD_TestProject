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
		private Image _activityFrame;

		[SerializeField, Required]
		private Image _targetFrame;

		[SerializeField, Required]
		private Image _influenceFrame;

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


		private float _influenceFrameAlpha;
		private Vector2 _bodyPosition;
		private bool _isTargetable;
		private bool _isInfluenced;


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
			_influenceFrameAlpha = _influenceFrame.color.a;
			_bodyPosition = _body.rectTransform.anchoredPosition;

			SetAlpha(_influenceFrame, 0f);
			_activityFrame.color = Transparent(__viewConfig.ActivityColor);
			_targetFrame.color = Transparent(__viewConfig.TargetColor);

			__unit.Hp.Subscribe(UpdateHp)
				.AddTo(this);
		}


		public void SetActivity(bool isActive)
			=> FadeTo(_activityFrame, isActive ? __viewConfig.ActivityColor.a : 0f);

		public void SetTargetable(bool isTargetable)
		{
			if (_isTargetable == isTargetable)
				return;

			_isTargetable = isTargetable;
			FadeTo(_targetFrame, isTargetable ? __viewConfig.TargetColor.a : 0f);
		}

		public void SetInfluenced(bool isInfluenced)
		{
			if (_isInfluenced == isInfluenced)
				return;

			_isInfluenced = isInfluenced;
			FadeTo(_influenceFrame, isInfluenced ? _influenceFrameAlpha : 0f);
		}

		public UniTask PlayFocusAsync(CancellationToken cancellationToken)
			=> PlayAsync(ScaleBody(__viewConfig.ParticipantScale), cancellationToken);

		public UniTask PlayUnfocusAsync(CancellationToken cancellationToken)
			=> PlayAsync(ScaleBody(1f), cancellationToken);

		public UniTask PlayLungeAsync(CancellationToken cancellationToken)
			=> PlayAsync(MoveBody(Facing * __viewConfig.LungeDistance, __viewConfig.LungeDuration), cancellationToken);

		public UniTask PlayLungeReturnAsync(CancellationToken cancellationToken)
			=> PlayAsync(MoveBody(0f, __viewConfig.LungeDuration), cancellationToken);

		public UniTask PlayRecoilAsync(CancellationToken cancellationToken)
			=> PlayAsync(SwayBody(-Facing * __viewConfig.RecoilDistance, __viewConfig.RecoilDuration), cancellationToken);

		public UniTask PlayCastAsync(CancellationToken cancellationToken)
			=> PlayAsync(_body.rectTransform.DOScale(_body.rectTransform.localScale * __viewConfig.CastScale, __viewConfig.CastDuration / 2f).SetLoops(2, LoopType.Yoyo), cancellationToken);

		public UniTask PlayHitAsync(CancellationToken cancellationToken)
			=> UniTask.WhenAll(PlayAsync(_body.rectTransform.DOShakeAnchorPos(__viewConfig.HitDuration, __viewConfig.HitShakeStrength), cancellationToken),
				FlashAsync(__viewConfig.HitColor, __viewConfig.HitDuration, cancellationToken));

		public UniTask PlayDodgeAsync(CancellationToken cancellationToken)
			=> PlayAsync(SwayBody(-Facing * __viewConfig.DodgeDistance, __viewConfig.DodgeDuration), cancellationToken);

		public UniTask PlayHealAsync(CancellationToken cancellationToken)
			=> FlashAsync(__viewConfig.HealColor, __viewConfig.HealDuration, cancellationToken);

		public UniTask PlayDeathAsync(CancellationToken cancellationToken)
			=> UniTask.WhenAll(PlayAsync(_canvasGroup.DOFade(__viewConfig.DeadAlpha, __viewConfig.DeathDuration), cancellationToken), PlayUnfocusAsync(cancellationToken));

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

		private Tween ScaleBody(float scale)
			=> _body.rectTransform.DOScale(scale, __viewConfig.ParticipantScaleDuration);

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

		private static Color Transparent(Color color)
		{
			color.a = 0f;

			return color;
		}


		public class Factory : PlaceholderFactory<BattleUnit, UnitView>
		{
		}
	}
}
