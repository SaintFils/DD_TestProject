using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DDTest.Battle
{
	public sealed class BattlePresenter : IBattlePresenter
	{
		private readonly BattlefieldView _battlefield;
		private readonly ProjectilePoolView _projectiles;
		private readonly InitiativeQueueView _initiativeQueue;
		private readonly RoundBannerView _roundBanner;
		private readonly BattleLogView _log;
		private readonly BattleResultView _result;


		public BattlePresenter(BattlefieldView battlefield, ProjectilePoolView projectiles, InitiativeQueueView initiativeQueue, RoundBannerView roundBanner,
			BattleLogView log, BattleResultView result)
		{
			_battlefield = battlefield;
			_projectiles = projectiles;
			_initiativeQueue = initiativeQueue;
			_roundBanner = roundBanner;
			_log = log;
			_result = result;
		}


		public UniTask PlayRoundStartAsync(int round, CancellationToken cancellationToken)
		{
			_log.AddRound(round);

			return _roundBanner.PlayAsync(round, cancellationToken);
		}

		public async UniTask PlayActionStartAsync(ActionResult result, CancellationToken cancellationToken)
		{
			if (result.IsSkip)
				return;

			await UniTask.WhenAll(GetParticipants(result).Select(view => view.PlayFocusAsync(cancellationToken)));
			await PlayWindupAsync(result, cancellationToken);
		}

		public async UniTask PlayActionEndAsync(ActionResult result, CancellationToken cancellationToken)
		{
			_log.AddAction(result);

			if (result.IsSkip)
				return;

			await UniTask.WhenAll(
				PlayFollowThroughAsync(result, cancellationToken),
				UniTask.WhenAll(result.Effects.Select(effect => PlayReactionAsync(effect, cancellationToken))));

			foreach (ActionEffect effect in result.Effects.Where(effect => !effect.Target.IsAlive))
				_log.AddDeath(effect.Target);

			await UniTask.WhenAll(GetParticipants(result).Select(view => PlayAftermathAsync(view, cancellationToken)));
		}

		public UniTask PlayBattleEndAsync(BattleSideId? winner, CancellationToken cancellationToken)
		{
			_log.AddResult(winner);

			return _result.ShowAsync(winner, cancellationToken);
		}

		private UniTask PlayWindupAsync(ActionResult result, CancellationToken cancellationToken)
		{
			UnitView actorView = _battlefield.GetView(result.Actor);
			bool isAnimated = actorView.HasAbilityAnimation(result.Ability);

			if (IsMelee(result.Ability))
				return UniTask.WhenAll(actorView.PlayLungeAsync(cancellationToken), actorView.PlayAbilityWindupAsync(result.Ability, cancellationToken));

			if (IsRanged(result.Ability))
			{
				return isAnimated
					? PlayAnimatedShotAsync(actorView, result, cancellationToken)
					: UniTask.WhenAll(ShootAsync(actorView, result, cancellationToken), actorView.PlayRecoilAsync(cancellationToken));
			}

			if (result.Ability.Effect == AbilityEffect.Heal)
				return isAnimated ? actorView.PlayAbilityWindupAsync(result.Ability, cancellationToken) : actorView.PlayCastAsync(cancellationToken);

			return UniTask.CompletedTask;
		}

		private async UniTask PlayAnimatedShotAsync(UnitView actorView, ActionResult result, CancellationToken cancellationToken)
		{
			await actorView.PlayAbilityWindupAsync(result.Ability, cancellationToken);
			await ShootAsync(actorView, result, cancellationToken);
		}

		private UniTask ShootAsync(UnitView actorView, ActionResult result, CancellationToken cancellationToken)
		{
			List<(Vector3 Target, bool IsMiss)> shots = result.Effects
				.Select(effect => (_battlefield.GetView(effect.Target).BodyCenter, effect.Outcome == ActionOutcome.Missed))
				.ToList();

			return _projectiles.FireAsync(actorView.BodyCenter, shots, cancellationToken);
		}

		private UniTask PlayFollowThroughAsync(ActionResult result, CancellationToken cancellationToken)
		{
			UnitView actorView = _battlefield.GetView(result.Actor);
			UniTask actorFollowThrough = actorView.PlayAbilityFollowThroughAsync(result.Ability, cancellationToken);

			if (IsMelee(result.Ability))
				return UniTask.WhenAll(actorView.PlayLungeReturnAsync(cancellationToken), actorFollowThrough);

			if (IsRanged(result.Ability))
				return UniTask.WhenAll(_projectiles.FinishAsync(cancellationToken), actorFollowThrough);

			return actorFollowThrough;
		}

		private UniTask PlayReactionAsync(ActionEffect effect, CancellationToken cancellationToken)
		{
			UnitView targetView = _battlefield.GetView(effect.Target);

			return effect.Outcome switch
			{
				ActionOutcome.Hit => targetView.PlayHitAsync(cancellationToken),
				ActionOutcome.Missed => targetView.PlayDodgeAsync(cancellationToken),
				ActionOutcome.Healed => targetView.PlayHealAsync(cancellationToken),
				_ => UniTask.CompletedTask
			};
		}

		private UniTask PlayAftermathAsync(UnitView view, CancellationToken cancellationToken)
		{
			if (view.Unit.IsAlive)
				return view.PlayUnfocusAsync(cancellationToken);

			return UniTask.WhenAll(
				view.PlayDeathAsync(cancellationToken),
				_initiativeQueue.PlayDeathAsync(view.Unit, cancellationToken));
		}

		private IEnumerable<UnitView> GetParticipants(ActionResult result)
			=> result.Effects
				.Select(effect => effect.Target)
				.Prepend(result.Actor)
				.Distinct()
				.Select(_battlefield.GetView);

		private static bool IsMelee(AbilityConfig ability)
			=> ability.Effect == AbilityEffect.Damage && ability.HasPositionRequirement && ability.ActorPosition == UnitPosition.Front;

		private static bool IsRanged(AbilityConfig ability)
			=> ability.Effect == AbilityEffect.Damage && !IsMelee(ability);
	}
}
