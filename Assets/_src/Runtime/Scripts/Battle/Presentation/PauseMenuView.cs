using DG.Tweening;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class PauseMenuView : MonoBehaviour
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;

		[Inject]
		private readonly BattlePause __pause;

		[Inject]
		private readonly BattleSession __session;


		[SerializeField, Required]
		private CanvasGroup _canvasGroup;

		[SerializeField, Required]
		private Button _resumeButton;

		[SerializeField, Required]
		private Button _restartButton;

		[SerializeField, Required]
		private Button _menuButton;


		private void Awake()
		{
			_canvasGroup.alpha = 0f;
			_canvasGroup.blocksRaycasts = false;

			__pause.IsPaused.Subscribe(OnPausedChanged)
				.AddTo(this);

			_resumeButton.OnClickAsObservable()
				.Subscribe(_ => __pause.Resume())
				.AddTo(this);

			_restartButton.OnClickAsObservable()
				.Subscribe(_ => __session.Restart())
				.AddTo(this);

			_menuButton.OnClickAsObservable()
				.Subscribe(_ => __session.ExitToMenu())
				.AddTo(this);
		}


		private void OnPausedChanged(bool isPaused)
		{
			if (isPaused)
				Show();
			else
				Hide();
		}

		private void Show()
		{
			_canvasGroup.blocksRaycasts = true;
			FadeTo(1f);
		}

		private void Hide()
		{
			_canvasGroup.blocksRaycasts = false;
			FadeTo(0f);
		}

		private void FadeTo(float alpha)
		{
			_canvasGroup.DOKill();
			_canvasGroup.DOFade(alpha, __viewConfig.PauseFadeDuration)
				.SetUpdate(true)
				.SetLink(gameObject);
		}
	}
}
