using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DDTest.Battle
{
	public sealed class AiController : ISideController
	{
		private readonly BattleRules _rules;
		private readonly AiConfig _config;


		public AiController(BattleRules rules, AiConfig config)
		{
			_rules = rules;
			_config = config;
		}


		public UniTask<BattleCommand> ChooseCommandAsync(BattleUnit actor, CancellationToken cancellationToken)
			=> UniTask.FromResult(ChooseCommand(actor));

		private BattleCommand ChooseCommand(BattleUnit actor)
		{
			List<AbilityConfig> usableAbilities = _rules.GetAbilities(actor)
				.Where(ability => _rules.CanUse(actor, ability))
				.ToList();

			if (TryChooseHeal(actor, usableAbilities, out BattleCommand heal))
				return heal;

			if (TryChooseAttack(actor, usableAbilities, out BattleCommand attack))
				return attack;

			return new BattleCommand(actor, _rules.SkipAbility, null);
		}

		private bool TryChooseHeal(BattleUnit actor, IEnumerable<AbilityConfig> abilities, out BattleCommand command)
		{
			command = default;

			AbilityConfig heal = abilities
				.Where(ability => ability.Effect == AbilityEffect.Heal)
				.OrderByDescending(ability => ability.Power)
				.FirstOrDefault();

			if (heal == null)
				return false;

			BattleUnit target = _rules.GetTargets(actor, heal)
				.Where(unit => GetHpRatio(unit) <= _config.HealThreshold)
				.OrderBy(GetHpRatio)
				.FirstOrDefault();

			if (target == null)
				return false;

			command = new BattleCommand(actor, heal, target);
			
			return true;
		}

		private bool TryChooseAttack(BattleUnit actor, IEnumerable<AbilityConfig> abilities, out BattleCommand command)
		{
			command = default;

			(AbilityConfig Ability, BattleUnit Target) best = abilities
				.Where(ability => ability.Effect == AbilityEffect.Damage)
				.SelectMany(ability => _rules.GetTargets(actor, ability).Select(target => (Ability: ability, Target: target)))
				.OrderByDescending(option => _rules.GetHitChance(actor, option.Target, option.Ability) * option.Ability.Power)
				.ThenBy(option => option.Target.Hp.CurrentValue)
				.FirstOrDefault();

			if (best.Ability == null)
				return false;

			command = new BattleCommand(actor, best.Ability, best.Target);
			
			return true;
		}

		private static float GetHpRatio(BattleUnit unit)
			=> (float)unit.Hp.CurrentValue / unit.MaxHp;
	}
}
