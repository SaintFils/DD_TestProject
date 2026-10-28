using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Ability Config", fileName = "ability_")]
	public partial class AbilityConfig : ScriptableObject
	{
		[SerializeField, GenPublicGetter]
		private string _displayName;

		[SerializeField, GenPublicGetter]
		private bool _requiresFrontline;

		[SerializeField, GenPublicGetter]
		private AbilityTargetSide _targetSide;

		[SerializeField, GenPublicGetter]
		private AbilityTargetFilter _targetFilter;

		[SerializeField, GenPublicGetter]
		private AbilityEffect _effect;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _power;

		[SerializeField, ShowIf(nameof(_effect), AbilityEffect.Damage), MinValue(0), GenPublicGetter]
		private int _frontlineAttackPenalty;
	}
}
