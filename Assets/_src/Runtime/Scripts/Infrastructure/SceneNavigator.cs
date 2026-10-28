using UnityEngine.SceneManagement;

namespace DDTest.Infrastructure
{
	public sealed class SceneNavigator
	{
		private const string MenuScene = "MenuScene";
		private const string BattleScene = "BattleScene";


		public void LoadMenu()
			=> SceneManager.LoadScene(MenuScene);

		public void LoadBattle()
			=> SceneManager.LoadScene(BattleScene);
	}
}
