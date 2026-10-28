using DDTest.Battle;
using DDTest.Infrastructure;
using DDTest.Menu;
using Zenject;

namespace DDTest
{
	public class ProjectInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container.Bind<SceneNavigator>()
				.AsSingle();

			Container.Bind<BattleLaunch>()
				.AsSingle();

			Container.Bind<TeamSelection>()
				.AsSingle();
		}
	}
}
