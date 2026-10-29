using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDTest.Battle
{
	[Serializable]
	public class SpriteAnimation
	{
		[SerializeField]
		private Sprite[] _frames = Array.Empty<Sprite>();

		[SerializeField, Min(1f)]
		private float _frameRate = 10f;


		public IReadOnlyList<Sprite> Frames
			=> _frames;

		public float FrameRate
			=> _frameRate;

		public bool HasFrames
			=> _frames.Length > 0;
	}
}
