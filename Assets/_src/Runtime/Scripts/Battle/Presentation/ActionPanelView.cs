using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class ActionPanelView : MonoBehaviour
	{
		[Inject]
		private readonly PlayerController __player;

		[Inject]
		private readonly BattleRules __rules;


		[SerializeField]
		private AbilityButtonView[] _buttons;


		private void Awake()
		{
			__player.AwaitingActor.Subscribe(ShowAbilities)
				.AddTo(this);

			__player.SelectedAbility.Subscribe(HighlightSelected)
				.AddTo(this);
		}


		private void ShowAbilities(BattleUnit actor)
		{
			List<AbilityConfig> abilities = actor != null ? __rules.GetAbilities(actor).ToList() : new List<AbilityConfig>();

			if (abilities.Count > _buttons.Length)
				Debug.LogWarning($"ActionPanelView | {actor} has {abilities.Count} abilities, only {_buttons.Length} buttons", this);

			for (int index = 0; index < _buttons.Length; index++)
			{
				if (index < abilities.Count)
					_buttons[index].Show(abilities[index], __rules.CanUse(actor, abilities[index]));
				else
					_buttons[index].Hide();
			}
		}

		private void HighlightSelected(AbilityConfig selectedAbility)
		{
			foreach (AbilityButtonView button in _buttons)
				button.SetSelected(button.Ability != null && button.Ability == selectedAbility);
		}
	}
}
