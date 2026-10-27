using System;
using DDTest.Infrastructure;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class BattleInstaller : MonoInstaller
	{
		[SerializeField, Required]
		private BattleConfig _battleConfig;

		[SerializeField, Required]
		private AiConfig _aiConfig;

		[SerializeField, Required]
		private BattleViewConfig _battleViewConfig;

		[SerializeField, Required]
		private BattlefieldView _battlefieldView;

		[SerializeField, Required]
		private UnitView _unitViewPrefab;


		public override void InstallBindings()
		{
			Container.BindInstance(_battleConfig);
			Container.BindInstance(_aiConfig);
			Container.BindInstance(_battleViewConfig);

			Container.Bind<IRandom>()
				.FromInstance(new SystemRandom(ResolveSeed()));

			Container.BindInterfacesAndSelfTo<BattleState>()
				.AsSingle();

			Container.Bind<BattleRules>()
				.AsSingle();

			Container.BindInterfacesAndSelfTo<PlayerController>()
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

			Container.BindFactory<BattleUnit, UnitView, UnitView.Factory>()
				.FromComponentInNewPrefab(_unitViewPrefab);

			Container.BindInterfacesTo<BattleEntryPoint>()
				.AsSingle();
		}

		private int ResolveSeed()
			=> _battleConfig.UseFixedSeed ? _battleConfig.Seed : Environment.TickCount;

		private static SideControllers CreateSideControllers(InjectContext context)
			=> new(context.Container.Resolve<PlayerController>(), context.Container.Resolve<AiController>());
	}
}
