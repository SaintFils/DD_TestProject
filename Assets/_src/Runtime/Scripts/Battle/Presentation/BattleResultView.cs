using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class BattleResultView : MonoBehaviour
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


		public UniTask ShowAsync(BattleSideId winner, CancellationToken cancellationToken)
		{
			_text.text = $"{winner} side wins";
			_text.color = __viewConfig.GetSideColor(winner);

			return _canvasGroup.DOFade(1f, __viewConfig.ResultFadeDuration)
				.SetLink(gameObject)
				.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
}
