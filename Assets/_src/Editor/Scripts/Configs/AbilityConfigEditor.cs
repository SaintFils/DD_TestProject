using DDTest.Battle;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace DDTest.Editor.Configs
{
	[CustomEditor(typeof(AbilityConfig))]
	public class AbilityConfigEditor : NaughtyInspector
	{
		private const int SummaryFontSize = 13;


		private GUIStyle _summaryStyle;


		public override void OnInspectorGUI()
		{
			_summaryStyle ??= new GUIStyle(EditorStyles.helpBox) { fontSize = SummaryFontSize, wordWrap = true };

			GUILayout.Label(Describe((AbilityConfig)target), _summaryStyle);
			base.OnInspectorGUI();
		}

		private static string Describe(AbilityConfig ability)
			=> string.Join("\n", DescribeActor(ability), DescribeTarget(ability), DescribeEffect(ability));

		private static string DescribeActor(AbilityConfig ability)
			=> ability.HasPositionRequirement ? $"Actor: {ability.ActorPosition}" : "Actor: any";

		private static string DescribeTarget(AbilityConfig ability)
		{
			string target = ability.TargetSide == AbilityTargetSide.Self ? "self" : $"{ability.TargetSide} {DescribePlacement(ability)}";
			string area = ability.Aoe switch
			{
				AbilityAoe.Behind => $" + {ability.AoeDepth} behind",
				AbilityAoe.InFront => $" + {ability.AoeDepth} in front",
				AbilityAoe.Around => $" + {ability.AoeDepth} around",
				AbilityAoe.WholeSide => ", whole side",
				_ => string.Empty
			};
			string exclusion = ability.TargetSide != AbilityTargetSide.Enemy && !ability.IncludeActor ? ", actor excluded" : string.Empty;

			return $"Target: {target}{area}{exclusion}";
		}

		private static string DescribePlacement(AbilityConfig ability)
			=> ability.TargetFilter == AbilityTargetFilter.Placement ? $"at {ability.TargetPosition}" : "(any living)";

		private static string DescribeEffect(AbilityConfig ability)
		{
			string effect = $"{ability.Effect} {ability.Power}";

			if (ability.Effect != AbilityEffect.Damage || ability.AttackPenalty == 0)
				return effect;

			return $"{effect}, attack penalty {ability.AttackPenalty} at {ability.AttackPenaltyPosition}";
		}
	}
}
