using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Menu
{
	public class DragGhostView : MonoBehaviour
	{
		[Inject]
		private readonly TeamBuilderConfig __config;


		[SerializeField, Required]
		private CanvasGroup _canvasGroup;

		[SerializeField, Required]
		private Image _portrait;


		private Vector3 _grabOffset;


		private void Awake()
		{
			_canvasGroup.alpha = 0f;
			_canvasGroup.blocksRaycasts = false;
		}


		public void Show(Sprite sprite, RectTransform source, PointerEventData eventData)
		{
			RectTransform rectTransform = (RectTransform)transform;
			Vector2 size = source.rect.size;

			_portrait.sprite = sprite;
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
			rectTransform.position = source.TransformPoint(source.rect.center);
			_grabOffset = rectTransform.position - GetPointerPosition(eventData);
			_canvasGroup.alpha = __config.DragGhostAlpha;
		}

		public void Follow(PointerEventData eventData)
			=> transform.position = GetPointerPosition(eventData) + _grabOffset;

		public void Hide()
			=> _canvasGroup.alpha = 0f;

		private Vector3 GetPointerPosition(PointerEventData eventData)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform.parent, eventData.position, eventData.pressEventCamera, out Vector3 position);

			return position;
		}
	}
}
