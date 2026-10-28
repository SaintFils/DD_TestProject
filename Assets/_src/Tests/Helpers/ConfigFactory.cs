using System;
using System.Collections.Generic;
using DDTest.Battle;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DDTest.Tests
{
	public sealed class ConfigFactory : IDisposable
	{
		private readonly List<Object> _created = new();


		public AbilityConfig Melee(int power = 10)
			=> Ability(AbilityTargetSide.Enemy, AbilityTargetFilter.Placement, AbilityEffect.Damage, power, actorPosition: UnitPosition.Front);

		public AbilityConfig Ranged(int power = 10, int frontlineAttackPenalty = 0)
			=> Ability(AbilityTargetSide.Enemy, AbilityTargetFilter.AnyLiving, AbilityEffect.Damage, power, attackPenalty: frontlineAttackPenalty);

		public AbilityConfig Heal(int power = 10)
			=> Ability(AbilityTargetSide.Ally, AbilityTargetFilter.AnyLiving, AbilityEffect.Heal, power);

		public AbilityConfig Ability(
			AbilityTargetSide targetSide,
			AbilityTargetFilter targetFilter,
			AbilityEffect effect,
			int power,
			UnitPosition? actorPosition = null,
			UnitPosition targetPosition = UnitPosition.Front,
			int attackPenalty = 0,
			UnitPosition attackPenaltyPosition = UnitPosition.Front)
			=> Create<AbilityConfig>(serialized =>
			{
				serialized.FindProperty("_displayName").stringValue = $"{effect} {targetSide} {targetFilter}";
				serialized.FindProperty("_hasPositionRequirement").boolValue = actorPosition.HasValue;
				serialized.FindProperty("_actorPosition").intValue = (int)actorPosition.GetValueOrDefault();
				serialized.FindProperty("_targetSide").intValue = (int)targetSide;
				serialized.FindProperty("_targetFilter").intValue = (int)targetFilter;
				serialized.FindProperty("_targetPosition").intValue = (int)targetPosition;
				serialized.FindProperty("_effect").intValue = (int)effect;
				serialized.FindProperty("_power").intValue = power;
				serialized.FindProperty("_attackPenalty").intValue = attackPenalty;
				serialized.FindProperty("_attackPenaltyPosition").intValue = (int)attackPenaltyPosition;
			});

		public UnitConfig Unit(AbilityConfig[] abilities = null, int maxHp = 20, int speed = 5, int attack = 5, int defense = 5)
			=> Create<UnitConfig>(serialized =>
			{
				serialized.FindProperty("_displayName").stringValue = "Unit";
				serialized.FindProperty("_maxHp").intValue = maxHp;
				serialized.FindProperty("_speed").intValue = speed;
				serialized.FindProperty("_attack").intValue = attack;
				serialized.FindProperty("_defense").intValue = defense;
				SetObjects(serialized.FindProperty("_abilities"), abilities ?? Array.Empty<AbilityConfig>());
			});

		public TeamConfig Team(params UnitConfig[] slots)
			=> Create<TeamConfig>(serialized =>
			{
				UnitConfig[] fullSlots = new UnitConfig[TeamConfig.SlotCount];
				Array.Copy(slots, fullSlots, slots.Length);
				SetObjects(serialized.FindProperty("_slots"), fullSlots);
			});

		public BattleConfig Battle(TeamConfig leftTeam, TeamConfig rightTeam, bool reshuffleEqualSpeedEachRound = false)
			=> Create<BattleConfig>(serialized =>
			{
				serialized.FindProperty("_leftTeam").objectReferenceValue = leftTeam;
				serialized.FindProperty("_rightTeam").objectReferenceValue = rightTeam;
				serialized.FindProperty("_reshuffleEqualSpeedEachRound").boolValue = reshuffleEqualSpeedEachRound;
			});

		public AiConfig Ai(float healThreshold = 0.5f, float killBonus = 0f, float targetTolerance = 0f)
			=> Create<AiConfig>(serialized =>
			{
				serialized.FindProperty("_healThreshold").floatValue = healThreshold;
				serialized.FindProperty("_thinkDuration").vector2Value = Vector2.zero;
				serialized.FindProperty("_killBonus").floatValue = killBonus;
				serialized.FindProperty("_targetTolerance").floatValue = targetTolerance;
			});

		public void Dispose()
		{
			foreach (Object created in _created)
				Object.DestroyImmediate(created);
		}

		private T Create<T>(Action<SerializedObject> setup) where T : ScriptableObject
		{
			T instance = ScriptableObject.CreateInstance<T>();
			_created.Add(instance);

			using SerializedObject serialized = new(instance);
			setup(serialized);
			serialized.ApplyModifiedPropertiesWithoutUndo();

			return instance;
		}

		private static void SetObjects(SerializedProperty array, IReadOnlyList<Object> objects)
		{
			array.arraySize = objects.Count;

			for (int index = 0; index < objects.Count; index++)
				array.GetArrayElementAtIndex(index).objectReferenceValue = objects[index];
		}
	}
}
