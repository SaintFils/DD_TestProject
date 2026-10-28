using System.Linq;
using DDTest.Battle;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DDTest.Menu
{
	public class UnitInfoView : MonoBehaviour
	{
		[SerializeField, Required]
		private GameObject _content;

		[SerializeField, Required]
		private Image _portrait;

		[SerializeField, Required]
		private TMP_Text _nameText;

		[SerializeField, Required]
		private TMP_Text _hpText;

		[SerializeField, Required]
		private TMP_Text _speedText;

		[SerializeField, Required]
		private TMP_Text _attackText;

		[SerializeField, Required]
		private TMP_Text _defenseText;

		[SerializeField, Required]
		private TMP_Text _abilitiesText;


		private void Awake()
			=> _content.SetActive(false);


		public void Show(UnitConfig unit)
		{
			_portrait.sprite = unit.Sprite;
			_nameText.text = unit.DisplayName;
			_hpText.text = $"HP: {unit.MaxHp.ToString()}";
			_speedText.text = $"Speed: {unit.Speed.ToString()}";
			_attackText.text = $"Attack: {unit.Attack.ToString()}";
			_defenseText.text = $"Defense: {unit.Defense.ToString()}";
			_abilitiesText.text = string.Join("\n", unit.Abilities.Select(ability => ability.DisplayName));
			_content.SetActive(true);
		}
	}
}
