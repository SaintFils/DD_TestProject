using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class QueueCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;

		[Inject]
		private readonly PlayerController __player;


		[SerializeField, Required]
		private RectTransform _content;

		[SerializeField, Required]
		private Image _portrait;

		[SerializeField, Required]
		private Image _sideMark;

		[SerializeField, Required]
		private Image _currentFrame;

		[SerializeField, Required]
		private CanvasGroup _canvasGroup;

		[SerializeField, Required]
		private LayoutElement _layoutElement;


		private Vector2 _contentPosition;
		private Color _portraitColor;
		private float _width;


		public BattleUnit Unit { get; private set; }


		private void Awake()
		{
			_contentPosition = _content.anchoredPosition;
			_portraitColor = _portrait.color;
			_width = _layoutElement.preferredWidth;
		}


		public void Show(BattleUnit unit)
		{
			gameObject.SetActive(true);
			ReleaseUnit();
			KillTweens();

			Unit = unit;
			_portrait.sprite = unit.Config.Sprite;
			_portrait.color = _portraitColor;
			_sideMark.color = __viewConfig.GetSideColor(unit.Side);
			_currentFrame.color = WithAlpha(__viewConfig.ActivityColor, 0f);
			_content.anchoredPosition = _contentPosition;
			_content.localScale = Vector3.one;
			_canvasGroup.alpha = 1f;
			_canvasGroup.blocksRaycasts = true;
			_layoutElement.preferredWidth = _width;
		}

		public void Hide()
		{
			ReleaseUnit();
			KillTweens();

			Unit = null;
			gameObject.SetActive(false);
		}

		public void SetState(bool isCurrent, bool isActed)
		{
			if (!Unit.IsAlive)
				return;

			float duration = __viewConfig.HighlightFadeDuration;

			KillTweens();
			_content.DOScale(isCurrent ? __viewConfig.CurrentCardScale : 1f, duration)
				.SetLink(gameObject);
			_currentFrame.DOFade(isCurrent ? __viewConfig.ActivityColor.a : 0f, duration)
				.SetLink(gameObject);
			_portrait.DOColor(isActed ? __viewConfig.ActedCardColor : _portraitColor, duration)
				.SetLink(gameObject);
		}

		public UniTask PlayDeathAsync(CancellationToken cancellationToken)
		{
			ReleaseUnit();
			KillTweens();
			_canvasGroup.blocksRaycasts = false;

			return DOTween.Sequence()
				.Append(_content.DOScale(__viewConfig.CardDeathScale, __viewConfig.CardDeathPopDuration))
				.Append(_content.DOAnchorPosY(_contentPosition.y - __viewConfig.CardDeathDrop, __viewConfig.CardDeathDuration))
				.Join(_canvasGroup.DOFade(0f, __viewConfig.CardDeathDuration))
				.Append(DOTween.To(() => _layoutElement.preferredWidth, width => _layoutElement.preferredWidth = width, 0f, __viewConfig.CardCollapseDuration))
				.SetLink(gameObject)
				.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (Unit != null)
				__player.InspectUnit(Unit);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (Unit != null)
				__player.UninspectUnit(Unit);
		}

		private void ReleaseUnit()
		{
			if (Unit != null)
				__player.UninspectUnit(Unit);
		}

		private void KillTweens()
		{
			_content.DOKill();
			_currentFrame.DOKill();
			_portrait.DOKill();
			_canvasGroup.DOKill();
		}

		private static Color WithAlpha(Color color, float alpha)
		{
			color.a = alpha;

			return color;
		}
	}
}
