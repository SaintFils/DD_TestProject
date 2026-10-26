using UnityEditor;
using UnityEngine;


namespace DDTest.Editor.UI
{
	public static class AnchorsToRect
	{
		private const float MinParentSize = 0.01f;


		[MenuItem("Tools/UI/Anchors To Rect _F1")]
		private static void ApplyToSelection()
		{
			foreach (GameObject gameObject in Selection.gameObjects)
			{
				if (gameObject.TryGetComponent(out RectTransform rectTransform))
					Apply(rectTransform);
			}
		}


		private static void Apply(RectTransform rectTransform)
		{
			if (rectTransform.parent is not RectTransform parent)
				return;

			Vector2 parentSize = parent.rect.size;
			
			if (Mathf.Abs(parentSize.x) < MinParentSize || Mathf.Abs(parentSize.y) < MinParentSize)
			{
				Debug.LogWarning($"AnchorsToRect | Skip <{rectTransform.name}>: parent <{parent.name}> has zero size {parentSize}", rectTransform);
				return;
			}

			Vector2 offsetMin = rectTransform.offsetMin;
			Vector2 offsetMax = rectTransform.offsetMax;

			Undo.RecordObject(rectTransform, "Anchors To Rect");
			
			rectTransform.anchorMin += new Vector2(offsetMin.x / parentSize.x, offsetMin.y / parentSize.y);
			rectTransform.anchorMax += new Vector2(offsetMax.x / parentSize.x, offsetMax.y / parentSize.y);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
	}
}
