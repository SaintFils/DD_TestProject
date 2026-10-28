using NaughtyAttributes;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Ability Config", fileName = "ability_")]
	public partial class AbilityConfig : ScriptableObject
	{
		[SerializeField, GenPublicGetter]
		private string _displayName;

		[SerializeField, GenPublicGetter]
		private bool _hasPositionRequirement;

		[SerializeField, ShowIf(nameof(_hasPositionRequirement)), GenPublicGetter]
		private UnitPosition _actorPosition;

		[SerializeField, GenPublicGetter]
		private AbilityTargetSide _targetSide;

		[SerializeField, HideIf(nameof(_targetSide), AbilityTargetSide.Enemy), GenPublicGetter]
		private bool _includeActor = true;

		[SerializeField, HideIf(nameof(_targetSide), AbilityTargetSide.Self), GenPublicGetter]
		private AbilityTargetFilter _targetFilter;

		[SerializeField, ShowIf(nameof(HasTargetPosition)), GenPublicGetter]
		private UnitPosition _targetPosition;

		[SerializeField, GenPublicGetter]
		private AbilityAoe _aoe;

		[SerializeField, ShowIf(nameof(HasAoeDepth)), MinValue(1), GenPublicGetter]
		private int _aoeDepth = 1;

		[SerializeField, GenPublicGetter]
		private AbilityEffect _effect;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _power;

		[SerializeField, ShowIf(nameof(_effect), AbilityEffect.Damage), MinValue(0), GenPublicGetter]
		private int _attackPenalty;

		[SerializeField, ShowIf(nameof(HasAttackPenalty)), GenPublicGetter]
		private UnitPosition _attackPenaltyPosition;


		private bool HasTargetPosition()
			=> _targetSide != AbilityTargetSide.Self && _targetFilter == AbilityTargetFilter.Placement;

		private bool HasAoeDepth()
			=> _aoe is AbilityAoe.Behind or AbilityAoe.InFront or AbilityAoe.Around;

		private bool HasAttackPenalty()
			=> _effect == AbilityEffect.Damage && _attackPenalty > 0;
	}
}
