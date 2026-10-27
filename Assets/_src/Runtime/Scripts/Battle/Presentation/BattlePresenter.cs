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

		public UniTask PlayActionStartAsync(ActionResult result, CancellationToken cancellationToken)
		{
			if (result.Target == null)
				return UniTask.CompletedTask;

			UnitView actorView = _battlefield.GetView(result.Actor);
			UnitView targetView = _battlefield.GetView(result.Target);

			targetView.SetActionTarget(true);

			return PlayWindupAsync(result, actorView, targetView, cancellationToken);
		}

		public async UniTask PlayActionEndAsync(ActionResult result, CancellationToken cancellationToken)
		{
			_log.AddAction(result);

			if (result.Target == null)
				return;

			UnitView actorView = _battlefield.GetView(result.Actor);
			UnitView targetView = _battlefield.GetView(result.Target);

			await UniTask.WhenAll(
				PlayFollowThroughAsync(result, actorView, cancellationToken),
				PlayReactionAsync(result, targetView, cancellationToken));

			targetView.SetActionTarget(false);

			if (!result.Target.IsAlive)
			{
				_log.AddDeath(result.Target);
				await UniTask.WhenAll(
					targetView.PlayDeathAsync(cancellationToken),
					_initiativeQueue.PlayDeathAsync(result.Target, cancellationToken));
			}
		}

		public UniTask PlayBattleEndAsync(BattleSideId winner, CancellationToken cancellationToken)
		{
			_log.AddWinner(winner);

			return _result.ShowAsync(winner, cancellationToken);
		}

		private UniTask PlayWindupAsync(ActionResult result, UnitView actorView, UnitView targetView, CancellationToken cancellationToken)
		{
			if (IsMelee(result.Ability))
				return actorView.PlayLungeAsync(cancellationToken);

			if (IsRanged(result.Ability))
				return UniTask.WhenAll(ShootAsync(result, actorView, targetView, cancellationToken), actorView.PlayRecoilAsync(cancellationToken));

			if (result.Ability.Effect == AbilityEffect.Heal)
				return actorView.PlayCastAsync(cancellationToken);

			return UniTask.CompletedTask;
		}

		private UniTask ShootAsync(ActionResult result, UnitView actorView, UnitView targetView, CancellationToken cancellationToken)
		{
			if (result.Outcome == ActionOutcome.Missed)
				return _projectile.FlyPastAsync(actorView.BodyCenter, targetView.BodyCenter, cancellationToken);

			return _projectile.FlyToAsync(actorView.BodyCenter, targetView.BodyCenter, cancellationToken);
		}

		private UniTask PlayFollowThroughAsync(ActionResult result, UnitView actorView, CancellationToken cancellationToken)
		{
			if (IsMelee(result.Ability))
				return actorView.PlayLungeReturnAsync(cancellationToken);

			if (!IsRanged(result.Ability))
				return UniTask.CompletedTask;

			if (result.Outcome == ActionOutcome.Missed)
				return _projectile.FlyAwayAsync(cancellationToken);

			_projectile.Hide();

			return UniTask.CompletedTask;
		}

		private static UniTask PlayReactionAsync(ActionResult result, UnitView targetView, CancellationToken cancellationToken)
			=> result.Outcome switch
			{
				ActionOutcome.Hit => targetView.PlayHitAsync(cancellationToken),
				ActionOutcome.Missed => targetView.PlayDodgeAsync(cancellationToken),
				ActionOutcome.Healed => targetView.PlayHealAsync(cancellationToken),
				_ => UniTask.CompletedTask
			};

		private static bool IsMelee(AbilityConfig ability)
			=> ability.Effect == AbilityEffect.Damage && ability.RequiresFrontline;

		private static bool IsRanged(AbilityConfig ability)
			=> ability.Effect == AbilityEffect.Damage && !ability.RequiresFrontline;
	}
}
