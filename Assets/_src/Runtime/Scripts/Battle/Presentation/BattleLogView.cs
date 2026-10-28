using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class BattleLogView : MonoBehaviour
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;


		[SerializeField, Required]
		private ScrollRect _scrollRect;

		[SerializeField, Required]
		private TMP_Text _linePrefab;


		private readonly Queue<TMP_Text> _lines = new();


		public void AddBattleStart(int seed)
			=> AddLine($"Battle started, seed {seed}", __viewConfig.LogMutedColor);

		public void AddRound(int round)
			=> AddLine($"<b>Round {round}</b>", _linePrefab.color);

		public void AddAction(ActionResult result)
			=> AddLine(Describe(result), _linePrefab.color);

		public void AddDeath(BattleUnit unit)
			=> AddLine($"{Name(unit)}: dies", _linePrefab.color);

		public void AddResult(BattleSideId? winner)
		{
			if (winner.HasValue)
				AddLine($"{Colorize($"{winner.Value} side", __viewConfig.GetSideColor(winner.Value))} wins", _linePrefab.color);
			else
				AddLine("Draw: round limit reached", _linePrefab.color);
		}

		private void AddLine(string text, Color color)
		{
			bool isAtBottom = IsScrolledToBottom();
			TMP_Text line = TakeLine();

			line.text = text;
			line.color = color;
			line.transform.SetAsLastSibling();

			if (!isAtBottom)
				return;

			LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
			_scrollRect.verticalNormalizedPosition = 0f;
		}

		private bool IsScrolledToBottom()
			=> _scrollRect.content.rect.height <= _scrollRect.viewport.rect.height || _scrollRect.verticalNormalizedPosition <= 0.01f;

		private TMP_Text TakeLine()
		{
			TMP_Text line = _lines.Count < __viewConfig.LogMaxLines ? Instantiate(_linePrefab, _scrollRect.content) : _lines.Dequeue();

			_lines.Enqueue(line);

			return line;
		}

		private string Describe(ActionResult result)
		{
			if (result.IsSkip)
				return $"{Name(result.Actor)}: skips the turn";

			IEnumerable<string> effects = result.Effects.Select(effect => Describe(effect, result.Ability));

			return $"{Name(result.Actor)}: {string.Join(", ", effects)}";
		}

		private string Describe(ActionEffect effect, AbilityConfig ability)
			=> effect.Outcome switch
			{
				ActionOutcome.Hit => $"{Colorize("hits", __viewConfig.HitColor)} {Name(effect.Target)} with {ability.DisplayName} for {effect.Amount} {HitChance(effect)}",
				ActionOutcome.Missed => $"{Colorize("misses", __viewConfig.LogMutedColor)} {Name(effect.Target)} with {ability.DisplayName} {HitChance(effect)}",
				_ => $"{Colorize("heals", __viewConfig.HealColor)} {Name(effect.Target)} with {ability.DisplayName} for {effect.Amount}"
			};

		private string Name(BattleUnit unit)
			=> Colorize($"{unit.Config.DisplayName} {unit.SlotIndex + 1}", __viewConfig.GetSideColor(unit.Side));

		private static string HitChance(ActionEffect effect)
			=> $"(Hit chance: {Mathf.RoundToInt(effect.HitChance * 100f)}%)";

		private static string Colorize(string text, Color color)
			=> $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
	}
}
