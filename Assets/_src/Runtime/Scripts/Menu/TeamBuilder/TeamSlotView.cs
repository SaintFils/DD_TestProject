using DDTest.Battle;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace DDTest.Menu
{
	public class TeamSlotView : MonoBehaviour
	{
		[SerializeField, Required]
		private Image _portrait;

		[SerializeField, Required]
		private Button _button;


		public Observable<Unit> Clicked
			=> _button.OnClickAsObservable();


		public void Show(UnitConfig unit)
		{
			bool hasUnit = unit != null;

			_portrait.sprite = hasUnit ? unit.Sprite : null;
			_portrait.enabled = hasUnit;
			_button.interactable = hasUnit;
		}
	}
}
