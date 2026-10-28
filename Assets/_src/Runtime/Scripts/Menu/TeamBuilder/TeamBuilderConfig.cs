using System.Collections.Generic;
using DDTest.Battle;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Menu
{
	[CreateAssetMenu(menuName = "DDTest/Menu/Team Builder Config", fileName = "team_builder_config")]
	public partial class TeamBuilderConfig : ScriptableObject
	{
		[SerializeField]
		private List<UnitConfig> _roster = new();

		[SerializeField, Range(0f, 1f), GenPublicGetter]
		private float _dragGhostAlpha = 0.6f;


		public IReadOnlyList<UnitConfig> Roster
			=> _roster;
	}
}
