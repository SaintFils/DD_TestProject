using System.Collections.Generic;
using DDTest.Battle;
using UnityEngine;

namespace DDTest.Menu
{
	[CreateAssetMenu(menuName = "DDTest/Menu/Team Builder Config", fileName = "team_builder_config")]
	public class TeamBuilderConfig : ScriptableObject
	{
		[SerializeField]
		private List<UnitConfig> _roster = new();


		public IReadOnlyList<UnitConfig> Roster
			=> _roster;
	}
}
