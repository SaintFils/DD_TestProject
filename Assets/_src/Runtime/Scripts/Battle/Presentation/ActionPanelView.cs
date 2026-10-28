using System;
using System.Collections.Generic;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;
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

		[SerializeField, Required]
		private Button _skipButton;


		private void Awake()
		{
			__player.AwaitingActor.Subscribe(ShowAbilities)
				.AddTo(this);

			__player.SelectedAbility.Subscribe(HighlightSelected)
				.AddTo(this);

			_skipButton.OnClickAsObservable()
				.Subscribe(_ => __player.Skip())
				.AddTo(this);
		}


		private void ShowAbilities(BattleUnit actor)
		{
			IReadOnlyList<AbilityConfig> abilities = actor != null ? actor.Abilities : Array.Empty<AbilityConfig>();

			if (abilities.Count > _buttons.Length)
				Debug.LogWarning($"ActionPanelView | {actor} has {abilities.Count} abilities, only {_buttons.Length} buttons", this);

			for (int index = 0; index < _buttons.Length; index++)
			{
				if (index < abilities.Count)
					_buttons[index].Show(abilities[index], __rules.CanUse(actor, abilities[index]));
				else
					_buttons[index].Hide();
			}

			_skipButton.gameObject.SetActive(actor != null);
		}

		private void HighlightSelected(AbilityConfig selectedAbility)
		{
			foreach (AbilityButtonView button in _buttons)
				button.SetSelected(button.Ability != null && button.Ability == selectedAbility);
		}
	}
}
