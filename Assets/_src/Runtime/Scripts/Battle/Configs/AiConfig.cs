using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/AI Config", fileName = "ai_config")]
	public partial class AiConfig : ScriptableObject
	{
		[SerializeField, Range(0f, 1f), GenPublicGetter]
		private float _healThreshold = 0.5f;
	}
}
