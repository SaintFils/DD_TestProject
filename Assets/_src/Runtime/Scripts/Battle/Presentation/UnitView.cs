using System.Collections.Generic;
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


		private UnitAnimationSet _animations;
		private Tween _idleTween;
		private float _influenceFrameAlpha;
		private Vector2 _bodyPosition;
		private float _bodyFlip = 1f;
		private bool _isTargetable;
		private bool _isInfluenced;


		public BattleUnit Unit
			=> __unit;

		public Vector3 BodyCenter
		{
			get
			{
				RectTransform rectTransform = (RectTransform)transform;

				return rectTransform.TransformPoint(rectTransform.rect.center);
			}
		}

		private float Facing
			=> __unit.Side == BattleSideId.Left ? 1f : -1f;

		private SpriteAnimation IdleAnimation
			=> _animations != null && _animations.Idle.HasFrames ? _animations.Idle : null;

		private SpriteAnimation DeathAnimation
			=> _animations != null && _animations.Death.HasFrames ? _animations.Death : null;


		private void Awake()
		{
			_animations = __unit.Config.Animations;
			_nameText.text = __unit.Config.DisplayName;
			_influenceFrameAlpha = _influenceFrame.color.a;

			if (_animations != null)
				ApplyAnimationLayout();

			_bodyPosition = _body.rectTransform.anchoredPosition;

			SetAlpha(_influenceFrame, 0f);
			_activityFrame.color = Transparent(__viewConfig.ActivityColor);
			_targetFrame.color = Transparent(__viewConfig.TargetColor);

			__unit.Hp.Subscribe(UpdateHp)
				.AddTo(this);

			PlayIdle();
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

		public bool HasAbilityAnimation(AbilityConfig ability)
			=> GetAbilityAnimation(ability) != null;

		public UniTask PlayAbilityWindupAsync(AbilityConfig ability, CancellationToken cancellationToken)
		{
			AbilityAnimation animation = GetAbilityAnimation(ability);

			if (animation == null)
				return UniTask.CompletedTask;

			StopIdle();

			return PlayAsync(PlayFrames(animation, 0, animation.ImpactFrame + 1), cancellationToken);
		}

		public async UniTask PlayAbilityFollowThroughAsync(AbilityConfig ability, CancellationToken cancellationToken)
		{
			AbilityAnimation animation = GetAbilityAnimation(ability);

			if (animation == null)
				return;

			if (animation.ImpactFrame + 1 < animation.Frames.Count)
				await PlayAsync(PlayFrames(animation, animation.ImpactFrame + 1, animation.Frames.Count), cancellationToken);

			PlayIdle();
		}

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
		{
			SpriteAnimation death = DeathAnimation;

			StopIdle();

			Tween dying = death != null
				? PlayFrames(death, 0, death.Frames.Count)
				: _canvasGroup.DOFade(__viewConfig.DeadAlpha, __viewConfig.DeathDuration);

			return UniTask.WhenAll(PlayAsync(dying, cancellationToken), PlayUnfocusAsync(cancellationToken));
		}

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
			=> _body.rectTransform.DOScale(new Vector3(scale * _bodyFlip, scale, scale), __viewConfig.ParticipantScaleDuration);

		private void ApplyAnimationLayout()
		{
			RectTransform body = _body.rectTransform;
			Vector2 offset = _animations.BodyOffset;

			_bodyFlip = Facing;
			body.anchorMin = new Vector2(0.5f, 0.5f);
			body.anchorMax = new Vector2(0.5f, 0.5f);
			body.sizeDelta = _animations.BodySize;
			body.anchoredPosition = new Vector2(offset.x * _bodyFlip, offset.y);
			body.localScale = new Vector3(_bodyFlip, 1f, 1f);
		}

		private void PlayIdle()
		{
			SpriteAnimation idle = IdleAnimation;

			if (idle == null)
			{
				_body.sprite = __unit.Config.Sprite;

				return;
			}

			_idleTween = PlayFrames(idle, 0, idle.Frames.Count)
				.SetLoops(-1)
				.SetLink(gameObject);

			_idleTween.Goto(UnityEngine.Random.Range(0f, _idleTween.Duration(false)), true);
		}

		private void StopIdle()
		{
			_idleTween?.Kill();
			_idleTween = null;
		}

		private Tween PlayFrames(SpriteAnimation animation, int firstFrame, int endFrame)
		{
			_body.sprite = animation.Frames[firstFrame];

			return DOVirtual.Float(firstFrame, endFrame, (endFrame - firstFrame) / animation.FrameRate, frame => _body.sprite = animation.Frames[Mathf.Min((int)frame, endFrame - 1)])
				.SetEase(Ease.Linear);
		}

		private AbilityAnimation GetAbilityAnimation(AbilityConfig ability)
		{
			if (_animations == null)
				return null;

			IReadOnlyList<AbilityConfig> abilities = __unit.Abilities;

			for (int index = 0; index < abilities.Count; index++)
			{
				if (abilities[index] == ability)
					return _animations.GetAbilityAnimation(index);
			}

			return null;
		}

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
