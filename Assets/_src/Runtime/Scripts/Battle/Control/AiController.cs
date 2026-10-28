using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DDTest.Infrastructure;

namespace DDTest.Battle
{
	public sealed class AiController : ISideController
	{
		private readonly BattleRules _rules;
		private readonly AiConfig _config;
		private readonly IRandom _random;


		public AiController(BattleRules rules, AiConfig config, IRandom random)
		{
			_rules = rules;
			_config = config;
			_random = random;
		}


		public async UniTask<BattleCommand> ChooseCommandAsync(BattleUnit actor, CancellationToken cancellationToken)
		{
			await ThinkAsync(cancellationToken);

			return ChooseCommand(actor);
		}

		private UniTask ThinkAsync(CancellationToken cancellationToken)
		{
			float duration = UnityEngine.Random.Range(_config.ThinkDuration.x, _config.ThinkDuration.y);

			if (duration <= 0f)
				return UniTask.CompletedTask;

			return UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken);
		}

		private BattleCommand ChooseCommand(BattleUnit actor)
		{
			List<AbilityConfig> usableAbilities = actor.Abilities
				.Where(ability => _rules.CanUse(actor, ability))
				.ToList();

			if (TryChooseHeal(actor, usableAbilities, out BattleCommand heal))
				return heal;

			if (TryChooseAttack(actor, usableAbilities, out BattleCommand attack))
				return attack;

			return BattleCommand.Skip(actor);
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

			List<(AbilityConfig Ability, BattleUnit Target, float Score)> options = abilities
				.Where(ability => ability.Effect == AbilityEffect.Damage)
				.SelectMany(ability => _rules.GetTargets(actor, ability).Select(target => (Ability: ability, Target: target, Score: GetAttackScore(actor, ability, target))))
				.OrderByDescending(option => option.Score)
				.ThenBy(option => option.Target.Hp.CurrentValue)
				.ToList();

			if (options.Count == 0)
				return false;

			float threshold = options[0].Score * (1f - _config.TargetTolerance);
			int candidateCount = 1 + options.Skip(1).TakeWhile(option => option.Score > threshold).Count();
			(AbilityConfig Ability, BattleUnit Target, float Score) choice = options[_random.Next(0, candidateCount)];

			command = new BattleCommand(actor, choice.Ability, choice.Target);

			return true;
		}

		private float GetAttackScore(BattleUnit actor, AbilityConfig ability, BattleUnit target)
		{
			int hp = target.Hp.CurrentValue;
			float value = Math.Min(ability.Power, hp);

			if (ability.Power >= hp)
				value += _config.KillBonus;

			return _rules.GetHitChance(actor, target, ability) * value;
		}

		private static float GetHpRatio(BattleUnit unit)
			=> (float)unit.Hp.CurrentValue / unit.MaxHp;
	}
}
