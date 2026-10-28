using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Team Config", fileName = "team_")]
	public class TeamConfig : ScriptableObject
	{
		public const int SlotCount = 4;


		[SerializeField]
		private UnitConfig[] _slots = new UnitConfig[SlotCount];


		public IReadOnlyList<UnitConfig> Slots
			=> _slots;


		private void OnValidate()
		{
			if (_slots.Length != SlotCount)
				Array.Resize(ref _slots, SlotCount);
		}
	}
}
