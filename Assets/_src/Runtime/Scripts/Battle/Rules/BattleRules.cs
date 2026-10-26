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


		public IEnumerable<AbilityConfig> GetAbilities(BattleUnit actor)
			=> actor.Abilities.Append(_config.SkipAbility);

		public bool CanUse(BattleUnit actor, AbilityConfig ability)
		{
			if (!actor.IsAlive)
				return false;

			if (ability.RequiresFrontline && !IsFrontline(actor))
				return false;

			return ability.TargetSide == AbilityTargetSide.None || GetTargets(actor, ability).Count > 0;
		}

		public IReadOnlyList<BattleUnit> GetTargets(BattleUnit actor, AbilityConfig ability)
		{
			if (ability.TargetSide == AbilityTargetSide.None)
				return Array.Empty<BattleUnit>();

			BattleSide side = ability.TargetSide == AbilityTargetSide.Enemy
				? _battle.GetOpposingSide(actor.Side)
				: _battle.GetSide(actor.Side);

			if (ability.TargetFilter == AbilityTargetFilter.Frontline)
				return side.Frontline != null ? new[] { side.Frontline } : Array.Empty<BattleUnit>();

			return side.Units.Where(unit => unit.IsAlive).ToList();
		}

		public float GetHitChance(BattleUnit actor, BattleUnit target, AbilityConfig ability)
		{
			int attack = IsFrontline(actor) ? actor.Attack - ability.FrontlineAttackPenalty : actor.Attack;
			float chance = _config.BaseHitChance + (attack - target.Defense) * _config.HitChancePerPoint;

			return Math.Clamp(chance, _config.MinHitChance, _config.MaxHitChance);
		}

		public ActionResult Resolve(BattleCommand command)
		{
			BattleUnit actor = command.Actor;
			AbilityConfig ability = command.Ability;
			BattleUnit target = command.Target;

			if (!GetAbilities(actor).Contains(ability) || !CanUse(actor, ability))
				throw new InvalidOperationException($"{actor} can't use {ability.DisplayName}");

			if (ability.TargetSide == AbilityTargetSide.None)
			{
				if (target != null)
					throw new InvalidOperationException($"{ability.DisplayName} doesn't affect a target");

				return ActionResult.NoEffect(actor, ability, null);
			}

			if (!GetTargets(actor, ability).Contains(target))
				throw new InvalidOperationException($"{target} is not a valid target of {ability.DisplayName} for {actor}");

			return ability.Effect switch
			{
				AbilityEffect.Damage => ResolveDamage(actor, ability, target),
				AbilityEffect.Heal => ActionResult.Healed(actor, ability, target, Math.Min(ability.Power, target.MaxHp - target.Hp.CurrentValue)),
				_ => ActionResult.NoEffect(actor, ability, target)
			};
		}

		private ActionResult ResolveDamage(BattleUnit actor, AbilityConfig ability, BattleUnit target)
		{
			float hitChance = GetHitChance(actor, target, ability);

			if (_random.NextDouble() >= hitChance)
				return ActionResult.Missed(actor, ability, target, hitChance);

			return ActionResult.Hit(actor, ability, target, Math.Min(ability.Power, target.Hp.CurrentValue), hitChance);
		}

		private bool IsFrontline(BattleUnit unit)
			=> _battle.GetSide(unit.Side).Frontline == unit;
	}
}
