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


		public override void InstallBindings()
		{
			Container.Bind<IRandom>()
				.FromInstance(new SystemRandom(ResolveSeed()));

			Container.BindInterfacesTo<BattleEntryPoint>()
				.AsSingle();
		}

		private int ResolveSeed()
			=> _battleConfig.UseFixedSeed ? _battleConfig.Seed : Environment.TickCount;
	}
}
