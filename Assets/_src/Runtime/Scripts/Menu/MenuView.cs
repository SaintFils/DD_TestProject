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

		[Inject]
		private readonly TeamSelection __teamSelection;


		[SerializeField, Required]
		private GameObject _mainPanel;

		[SerializeField, Required]
		private Button _playButton;

		[SerializeField, Required]
		private Button _quickBattleButton;

		[SerializeField, Required]
		private Button _quitButton;

		[SerializeField, Required]
		private TeamBuilderView _teamBuilder;


		private void Awake()
		{
			_playButton.OnClickAsObservable()
				.Subscribe(_ => SetTeamBuilderShown(true))
				.AddTo(this);

			_quickBattleButton.OnClickAsObservable()
				.Subscribe(_ => StartBattle(null, isLeftAi: false, isRightAi: true))
				.AddTo(this);

#if UNITY_WEBGL
			_quitButton.gameObject.SetActive(false);
#else
			_quitButton.OnClickAsObservable()
				.Subscribe(_ => Quit())
				.AddTo(this);
#endif

			_teamBuilder.BackClicked
				.Subscribe(_ => SetTeamBuilderShown(false))
				.AddTo(this);

			_teamBuilder.PlayClicked
				.Subscribe(_ => StartCustomBattle())
				.AddTo(this);

			SetTeamBuilderShown(false);
		}


		private void SetTeamBuilderShown(bool isShown)
		{
			_mainPanel.SetActive(!isShown);
			_teamBuilder.gameObject.SetActive(isShown);
		}

		private void StartCustomBattle()
		{
			bool isLeftAi = __teamSelection.Get(BattleSideId.Left).IsAi;
			bool isRightAi = __teamSelection.Get(BattleSideId.Right).IsAi;

			StartBattle(__teamSelection.ToBattleTeams(), isLeftAi, isRightAi);
		}

		private void StartBattle(BattleTeams teams, bool isLeftAi, bool isRightAi)
		{
			__battleLaunch.Seed = null;
			__battleLaunch.Teams = teams;
			__battleLaunch.IsLeftAi = isLeftAi;
			__battleLaunch.IsRightAi = isRightAi;
			__navigator.LoadBattle();
		}

#if !UNITY_WEBGL
		private static void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
#endif
	}
}
