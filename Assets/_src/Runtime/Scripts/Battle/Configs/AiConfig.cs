using NaughtyAttributes;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/AI Config", fileName = "ai_config")]
	public partial class AiConfig : ScriptableObject
	{
		[SerializeField, Range(0f, 1f), GenPublicGetter]
		private float _healThreshold = 0.5f;

		[SerializeField, MinMaxSlider(0f, 5f), GenPublicGetter]
		private Vector2 _thinkDuration = new(0.5f, 2f);
	}
}
