using System;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Unit Animation Set", fileName = "animations_")]
	public partial class UnitAnimationSet : ScriptableObject
	{
		[SerializeField, GenPublicGetter]
		private Vector2 _bodySize = new(256f, 192f);

		[SerializeField, GenPublicGetter]
		private Vector2 _bodyOffset;

		[SerializeField, GenPublicGetter]
		private SpriteAnimation _idle = new();

		[SerializeField]
		private AbilityAnimation[] _abilityAnimations = Array.Empty<AbilityAnimation>();

		[SerializeField, GenPublicGetter]
		private SpriteAnimation _death = new();


		public AbilityAnimation GetAbilityAnimation(int abilityIndex)
		{
			if (abilityIndex < 0 || abilityIndex >= _abilityAnimations.Length || !_abilityAnimations[abilityIndex].HasFrames)
				return null;

			return _abilityAnimations[abilityIndex];
		}
	}
}
