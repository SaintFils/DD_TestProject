using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class BattleResultView : MonoBehaviour
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;

		[Inject]
		private readonly BattleSession __session;


		[SerializeField, Required]
		private CanvasGroup _canvasGroup;

		[SerializeField, Required]
		private TMP_Text _text;

		[SerializeField, Required]
		private Button _restartButton;

		[SerializeField, Required]
		private Button _menuButton;


		private Color _drawColor;


		private void Awake()
		{
			_drawColor = _text.color;
			_canvasGroup.alpha = 0f;
			_canvasGroup.blocksRaycasts = false;

			_restartButton.OnClickAsObservable()
				.Subscribe(_ => __session.Restart())
				.AddTo(this);

			_menuButton.OnClickAsObservable()
				.Subscribe(_ => __session.ExitToMenu())
				.AddTo(this);
		}


		public UniTask ShowAsync(BattleSideId? winner, CancellationToken cancellationToken)
		{
			_canvasGroup.blocksRaycasts = true;

			if (winner.HasValue)
			{
				_text.text = $"{winner.Value} side wins";
				_text.color = __viewConfig.GetSideColor(winner.Value);
			}
			else
			{
				_text.text = "Draw";
				_text.color = _drawColor;
			}

			return _canvasGroup.DOFade(1f, __viewConfig.ResultFadeDuration)
				.SetLink(gameObject)
				.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
}
