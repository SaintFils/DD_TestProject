using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DDTest.Battle
{
	public sealed class BattlePresenter : IBattlePresenter
	{
		private readonly BattlefieldView _battlefield;
		private readonly ProjectileView _projectile;
		private readonly InitiativeQueueView _initiativeQueue;
		private readonly RoundBannerView _roundBanner;
		private readonly BattleLogView _log;
		private readonly BattleResultView _result;


		public BattlePresenter(BattlefieldView battlefield, ProjectileView projectile, InitiativeQueueView initiativeQueue, RoundBannerView roundBanner, 
			BattleLogView log, BattleResultView result)
		{
			_battlefield = battlefield;
			_projectile = projectile;
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

		public UniTask PlayBattleEndAsync(BattleSideId winner, CancellationToken cancellationToken)
		{
			_log.AddWinner(winner);

			return _result.ShowAsync(winner, cancellationToken);
		}

		private UniTask PlayWindupAsync(ActionResult result, CancellationToken cancellationToken)
		{
			UnitView actorView = _battlefield.GetView(result.Actor);

			if (IsMelee(result.Ability))
				return actorView.PlayLungeAsync(cancellationToken);

			if (IsRanged(result.Ability))
				return UniTask.WhenAll(result.Effects
					.Select(effect => ShootAsync(actorView, effect, cancellationToken))
					.Append(actorView.PlayRecoilAsync(cancellationToken)));

			if (result.Ability.Effect == AbilityEffect.Heal)
				return actorView.PlayCastAsync(cancellationToken);

			return UniTask.CompletedTask;
		}

		private UniTask ShootAsync(UnitView actorView, ActionEffect effect, CancellationToken cancellationToken)
		{
			UnitView targetView = _battlefield.GetView(effect.Target);

			if (effect.Outcome == ActionOutcome.Missed)
				return _projectile.FlyPastAsync(actorView.BodyCenter, targetView.BodyCenter, cancellationToken);

			return _projectile.FlyToAsync(actorView.BodyCenter, targetView.BodyCenter, cancellationToken);
		}

		private UniTask PlayFollowThroughAsync(ActionResult result, CancellationToken cancellationToken)
		{
			if (IsMelee(result.Ability))
				return _battlefield.GetView(result.Actor).PlayLungeReturnAsync(cancellationToken);

			if (!IsRanged(result.Ability))
				return UniTask.CompletedTask;

			if (result.Effects.Any(effect => effect.Outcome == ActionOutcome.Missed))
				return _projectile.FlyAwayAsync(cancellationToken);

			_projectile.Hide();

			return UniTask.CompletedTask;
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
