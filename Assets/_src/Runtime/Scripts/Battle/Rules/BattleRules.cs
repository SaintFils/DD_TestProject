using System;
using System.Collections.Generic;
using System.Linq;
using DDTest.Infrastructure;

namespace DDTest.Battle
{
	public sealed class BattleRules
	{
		private readonly BattleState _battle;
		private readonly BattleConfig _config;
		private readonly IRandom _random;


		public BattleRules(BattleState battle, BattleConfig config, IRandom random)
		{
			_battle = battle;
			_config = config;
			_random = random;
		}


		public bool CanUse(BattleUnit actor, AbilityConfig ability)
		{
			if (!actor.IsAlive)
				return false;

			if (ability.HasPositionRequirement && !_battle.GetSide(actor.Side).IsAt(actor, ability.ActorPosition))
				return false;

			return GetTargets(actor, ability).Count > 0;
		}

		public IReadOnlyList<BattleUnit> GetTargets(BattleUnit actor, AbilityConfig ability)
		{
			BattleSide side = ability.TargetSide == AbilityTargetSide.Enemy
				? _battle.GetOpposingSide(actor.Side)
				: _battle.GetSide(actor.Side);

			if (ability.TargetFilter == AbilityTargetFilter.Placement)
			{
				BattleUnit unit = side.GetUnitAt(ability.TargetPosition);

				return unit != null ? new[] { unit } : Array.Empty<BattleUnit>();
			}

			return side.Units.Where(unit => unit.IsAlive).ToList();
		}

		public IReadOnlyList<BattleUnit> GetAffectedUnits(BattleUnit actor, AbilityConfig ability, BattleUnit target)
			=> new[] { target };

		public float GetHitChance(BattleUnit actor, BattleUnit target, AbilityConfig ability)
		{
			int attack = HasAttackPenalty(actor, ability) ? actor.Attack - ability.AttackPenalty : actor.Attack;
			float chance = _config.BaseHitChance + (attack - target.Defense) * _config.HitChancePerPoint;

			return Math.Clamp(chance, _config.MinHitChance, _config.MaxHitChance);
		}

		public ActionResult Resolve(BattleCommand command)
		{
			BattleUnit actor = command.Actor;
			AbilityConfig ability = command.Ability;
			BattleUnit target = command.Target;

			if (!actor.IsAlive)
				throw new InvalidOperationException($"{actor} is dead and can't act");

			if (command.IsSkip)
				return ActionResult.Skip(actor);

			if (!actor.Abilities.Contains(ability) || !CanUse(actor, ability))
				throw new InvalidOperationException($"{actor} can't use {ability.DisplayName}");

			if (!GetTargets(actor, ability).Contains(target))
				throw new InvalidOperationException($"{target} is not a valid target of {ability.DisplayName} for {actor}");

			List<ActionEffect> effects = GetAffectedUnits(actor, ability, target)
				.Select(unit => ResolveEffect(actor, ability, unit))
				.ToList();

			return new ActionResult(actor, ability, target, effects);
		}

		private ActionEffect ResolveEffect(BattleUnit actor, AbilityConfig ability, BattleUnit target)
			=> ability.Effect switch
			{
				AbilityEffect.Damage => ResolveDamage(actor, ability, target),
				AbilityEffect.Heal => ActionEffect.Healed(target, Math.Min(ability.Power, target.MaxHp - target.Hp.CurrentValue)),
				_ => throw new ArgumentOutOfRangeException(nameof(ability), ability.Effect, $"{ability.DisplayName} has an unknown effect")
			};

		private ActionEffect ResolveDamage(BattleUnit actor, AbilityConfig ability, BattleUnit target)
		{
			float hitChance = GetHitChance(actor, target, ability);

			if (_random.NextDouble() >= hitChance)
				return ActionEffect.Missed(target, hitChance);

			return ActionEffect.Hit(target, Math.Min(ability.Power, target.Hp.CurrentValue), hitChance);
		}

		private bool HasAttackPenalty(BattleUnit actor, AbilityConfig ability)
			=> ability.AttackPenalty > 0 && _battle.GetSide(actor.Side).IsAt(actor, ability.AttackPenaltyPosition);
	}
}
