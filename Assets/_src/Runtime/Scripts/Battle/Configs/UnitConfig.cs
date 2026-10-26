using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Unit Config", fileName = "unit_")]
	public partial class UnitConfig : ScriptableObject
	{
		[SerializeField, GenPublicGetter]
		private string _displayName;

		[SerializeField, GenPublicGetter]
		private Color _color = Color.white;

		[SerializeField, MinValue(1), GenPublicGetter]
		private int _maxHp = 20;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _speed;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _attack;

		[SerializeField, MinValue(0), GenPublicGetter]
		private int _defense;

		[SerializeField]
		private List<AbilityConfig> _abilities = new();


		public IReadOnlyList<AbilityConfig> Abilities
			=> _abilities;
	}
}
