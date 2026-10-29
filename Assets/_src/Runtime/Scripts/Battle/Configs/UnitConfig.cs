using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Unit Config", fileName = "unit_")]
	public partial class UnitConfig : ScriptableObject
	{
		public const int MaxAbilityCount = 3;


		[SerializeField, GenPublicGetter]
		private string _displayName;

		[SerializeField, ShowAssetPreview, GenPublicGetter]
		private Sprite _sprite;

		[SerializeField, GenPublicGetter]
		private UnitAnimationSet _animations;

		[SerializeField, MinValue(1), GenPublicGetter]
		private int _maxHp = 20;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _speed;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _attack;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _defense;

		[SerializeField, ValidateInput(nameof(HasValidAbilityCount), "3 abilities max!")]
		private List<AbilityConfig> _abilities = new();


		public IReadOnlyList<AbilityConfig> Abilities
			=> _abilities;


		private bool HasValidAbilityCount()
			=> _abilities.Count <= MaxAbilityCount;
	}
}
