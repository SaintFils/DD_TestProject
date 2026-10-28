using DDTest.Battle;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Menu
{
	public class RosterCardView : MonoBehaviour
	{
		[Inject]
		private readonly UnitConfig __unit;


		[SerializeField, Required]
		private Image _portrait;

		[SerializeField, Required]
		private Button _button;


		public Observable<UnitConfig> Clicked
			=> _button.OnClickAsObservable().Select(_ => __unit);


		private void Awake()
			=> _portrait.sprite = __unit.Sprite;


		public class Factory : PlaceholderFactory<UnitConfig, RosterCardView>
		{
		}
	}
}
