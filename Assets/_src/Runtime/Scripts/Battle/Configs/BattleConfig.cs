using NaughtyAttributes;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Battle Config", fileName = "battle_config")]
	public partial class BattleConfig : ScriptableObject
	{
		[SerializeField, GenPublicGetter]
		private bool _useFixedSeed;

		[SerializeField, ShowIf(nameof(_useFixedSeed)), GenPublicGetter]
		private int _seed;
	}
}
