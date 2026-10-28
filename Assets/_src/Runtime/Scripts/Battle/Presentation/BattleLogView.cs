using System.Collections.Generic;
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

		public void AddWinner(BattleSideId winner)
			=> AddLine($"{Colorize($"{winner} side", __viewConfig.GetSideColor(winner))} wins", _linePrefab.color);

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
			=> result.Outcome switch
			{
				ActionOutcome.Hit => $"{Name(result.Actor)}: {Colorize("hits", __viewConfig.HitColor)} {Name(result.Target)} with {result.Ability.DisplayName} for {result.Amount} {HitChance(result)}",
				ActionOutcome.Missed => $"{Name(result.Actor)}: {Colorize("misses", __viewConfig.LogMutedColor)} {Name(result.Target)} with {result.Ability.DisplayName} {HitChance(result)}",
				ActionOutcome.Healed => $"{Name(result.Actor)}: {Colorize("heals", __viewConfig.HealColor)} {Name(result.Target)} with {result.Ability.DisplayName} for {result.Amount}",
				_ => $"{Name(result.Actor)}: skips the turn"
			};

		private string Name(BattleUnit unit)
			=> Colorize($"{unit.Config.DisplayName} {unit.SlotIndex + 1}", __viewConfig.GetSideColor(unit.Side));

		private static string HitChance(ActionResult result)
			=> $"(Hit chance: {Mathf.RoundToInt(result.HitChance * 100f)}%)";

		private static string Colorize(string text, Color color)
			=> $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
	}
}
