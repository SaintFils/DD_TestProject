using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class BattleHudView : MonoBehaviour
	{
		[Inject]
		private readonly BattlePause __pause;


		[SerializeField, Required]
		private Button _pauseButton;


		private void Awake()
		{
			_pauseButton.OnClickAsObservable()
				.Subscribe(_ => __pause.Pause())
				.AddTo(this);
		}
	}
}
