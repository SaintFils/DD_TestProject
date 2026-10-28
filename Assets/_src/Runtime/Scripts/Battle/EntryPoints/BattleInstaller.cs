using System;
using DDTest.Infrastructure;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class BattleInstaller : MonoInstaller
	{
		[Inject]
		private readonly BattleLaunch __battleLaunch;


		[SerializeField, Required]
		private BattleConfig _battleConfig;

		[SerializeField, Required]
		private AiConfig _aiConfig;

		[SerializeField, Required]
		private BattleViewConfig _battleViewConfig;

		[SerializeField, Required]
		private BattlefieldView _battlefieldView;

		[SerializeField, Required]
		private ProjectilePoolView _projectilePoolView;

		[SerializeField, Required]
		private InitiativeQueueView _initiativeQueueView;

		[SerializeField, Required]
		private RoundBannerView _roundBannerView;

		[SerializeField, Required]
		private BattleLogView _battleLogView;

		[SerializeField, Required]
		private BattleResultView _battleResultView;

		[SerializeField, Required]
		private UnitView _unitViewPrefab;

		[SerializeField, Required]
		private ProjectileView _projectilePrefab;


		public override void InstallBindings()
		{
			Container.BindInstance(_battleConfig);
			Container.BindInstance(_aiConfig);
			Container.BindInstance(_battleViewConfig);

			Container.Bind<IRandom>()
				.FromInstance(new SystemRandom(ResolveSeed()));

			Container.Bind<BattleState>()
				.AsSingle();

			Container.Bind<BattleRules>()
				.AsSingle();

			Container.Bind<PlayerController>()
				.AsSingle();

			Container.BindInterfacesAndSelfTo<BattlePause>()
				.AsSingle();

			Container.BindInterfacesTo<BattleInput>()
				.AsSingle();

			Container.Bind<AiController>()
				.AsSingle();

			Container.Bind<SideControllers>()
				.FromMethod(CreateSideControllers)
				.AsSingle();

			Container.Bind<IBattlePresenter>()
				.To<BattlePresenter>()
				.AsSingle();

			Container.Bind<BattleLoop>()
				.AsSingle();

			Container.Bind<BattlefieldView>()
				.FromInstance(_battlefieldView);

			Container.Bind<ProjectilePoolView>()
				.FromInstance(_projectilePoolView);

			Container.Bind<InitiativeQueueView>()
				.FromInstance(_initiativeQueueView);

			Container.Bind<RoundBannerView>()
				.FromInstance(_roundBannerView);

			Container.Bind<BattleLogView>()
				.FromInstance(_battleLogView);

			Container.Bind<BattleResultView>()
				.FromInstance(_battleResultView);

			Container.BindFactory<BattleUnit, UnitView, UnitView.Factory>()
				.FromComponentInNewPrefab(_unitViewPrefab);

			Container.BindFactory<ProjectileView, ProjectileView.Factory>()
				.FromComponentInNewPrefab(_projectilePrefab);

			Container.BindInterfacesAndSelfTo<BattleSession>()
				.AsSingle();

			Container.BindInterfacesTo<BattleEntryPoint>()
				.AsSingle();
		}

		private int ResolveSeed()
		{
			__battleLaunch.Seed ??= _battleConfig.UseFixedSeed ? _battleConfig.Seed : Environment.TickCount;

			return __battleLaunch.Seed.Value;
		}

		private static SideControllers CreateSideControllers(InjectContext context)
			=> new(context.Container.Resolve<PlayerController>(), context.Container.Resolve<AiController>());
	}
}
