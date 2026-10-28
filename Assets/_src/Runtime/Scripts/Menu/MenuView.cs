using DDTest.Battle;
using DDTest.Infrastructure;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Menu
{
	public class MenuView : MonoBehaviour
	{
		[Inject]
		private readonly SceneNavigator __navigator;

		[Inject]
		private readonly BattleLaunch __battleLaunch;


		[SerializeField, Required]
		private Button _playButton;

		[SerializeField, Required]
		private Button _quitButton;


		private void Awake()
		{
			_playButton.OnClickAsObservable()
				.Subscribe(_ => Play())
				.AddTo(this);

			_quitButton.OnClickAsObservable()
				.Subscribe(_ => Quit())
				.AddTo(this);
		}


		private void Play()
		{
			__battleLaunch.Seed = null;
			__navigator.LoadBattle();
		}

		private static void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}
