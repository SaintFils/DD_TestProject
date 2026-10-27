using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Battle View Config", fileName = "battle_view_config")]
	public partial class BattleViewConfig : ScriptableObject
	{
		[SerializeField, GenPublicGetter]
		private Color _currentTurnColor = new(1f, 0.85f, 0.3f, 1f);

		[SerializeField, GenPublicGetter]
		private Color _selectionColor = new(0.3f, 0.9f, 1f, 1f);

		[SerializeField, Min(0f), GenPublicGetter]
		private float _highlightFadeDuration = 0.15f;

		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _actionTargetScale = 1.1f;

		[SerializeField, Range(0f, 1f), GenPublicGetter]
		private float _deadAlpha = 0.3f;
	}
}
