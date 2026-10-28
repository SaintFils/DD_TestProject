using System;
using DDTest.Battle;
using DDTest.Infrastructure;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace DDTest.Menu
{
	public class MenuInstaller : MonoInstaller
	{
		[SerializeField, Required]
		private TeamBuilderConfig _teamBuilderConfig;

		[SerializeField, Required]
		private RosterCardView _rosterCardPrefab;


		public override void InstallBindings()
		{
			Container.BindInstance(_teamBuilderConfig);

			Container.Bind<IRandom>()
				.FromInstance(new SystemRandom(Environment.TickCount));

			Container.BindFactory<UnitConfig, RosterCardView, RosterCardView.Factory>()
				.FromComponentInNewPrefab(_rosterCardPrefab);
		}
	}
}
