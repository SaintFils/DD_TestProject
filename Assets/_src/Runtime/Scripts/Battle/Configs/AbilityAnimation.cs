using System;
using UnityEngine;

namespace DDTest.Battle
{
	[Serializable]
	public class AbilityAnimation : SpriteAnimation
	{
		[SerializeField, Min(0)]
		private int _impactFrame;


		public int ImpactFrame
			=> Mathf.Min(_impactFrame, Frames.Count - 1);
	}
}
