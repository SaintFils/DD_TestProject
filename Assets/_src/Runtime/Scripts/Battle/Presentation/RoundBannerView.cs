using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class RoundBannerView : MonoBehaviour
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;


		[SerializeField, Required]
		private CanvasGroup _canvasGroup;

		[SerializeField, Required]
		private TMP_Text _text;


		private void Awake()
		{
			_canvasGroup.alpha = 0f;
			_canvasGroup.blocksRaycasts = false;
		}


		public UniTask PlayAsync(int round, CancellationToken cancellationToken)
		{
			float fadeDuration = __viewConfig.RoundBannerFadeDuration;

			_text.text = $"Round {round}";

			return DOTween.Sequence()
				.Append(_canvasGroup.DOFade(1f, fadeDuration))
				.AppendInterval(__viewConfig.RoundBannerHoldDuration)
				.Append(_canvasGroup.DOFade(0f, fadeDuration))
				.SetLink(gameObject)
				.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
}
