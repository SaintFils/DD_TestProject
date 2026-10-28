using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace DDTest.Battle
{
	public sealed class PlayerController : ISideController
	{
		private readonly BattleRules _rules;
		private readonly ReactiveProperty<BattleUnit> _awaitingActor = new();
		private readonly ReactiveProperty<AbilityConfig> _selectedAbility = new();
		private readonly ReactiveProperty<IReadOnlyList<BattleUnit>> _targetableUnits = new(Array.Empty<BattleUnit>());
		private readonly ReactiveProperty<IReadOnlyList<BattleUnit>> _affectedUnits = new(Array.Empty<BattleUnit>());
		private readonly ReactiveProperty<BattleUnit> _inspectedUnit = new();
		
		private AbilityConfig _previewedAbility;
		private BattleUnit _hoveredTarget;
		private UniTaskCompletionSource<BattleCommand> _commandSource;


		public ReadOnlyReactiveProperty<BattleUnit> AwaitingActor
			=> _awaitingActor;

		public ReadOnlyReactiveProperty<AbilityConfig> SelectedAbility
			=> _selectedAbility;

		public ReadOnlyReactiveProperty<IReadOnlyList<BattleUnit>> TargetableUnits
			=> _targetableUnits;

		public ReadOnlyReactiveProperty<IReadOnlyList<BattleUnit>> AffectedUnits
			=> _affectedUnits;

		public ReadOnlyReactiveProperty<BattleUnit> InspectedUnit
			=> _inspectedUnit;


		public PlayerController(BattleRules rules)
		{
			_rules = rules;
		}


		public UniTask<BattleCommand> ChooseCommandAsync(BattleUnit actor, CancellationToken cancellationToken)
		{
			_commandSource = new UniTaskCompletionSource<BattleCommand>();
			_awaitingActor.Value = actor;

			return _commandSource.Task.AttachExternalCancellation(cancellationToken);
		}

		public void PreviewAbility(AbilityConfig ability)
		{
			_previewedAbility = ability;
			RefreshTargetableUnits();
		}

		public void ClearPreview(AbilityConfig ability)
		{
			if (_previewedAbility != ability)
				return;

			_previewedAbility = null;
			RefreshTargetableUnits();
		}

		public void SelectAbility(AbilityConfig ability)
		{
			BattleUnit actor = _awaitingActor.Value;

			if (actor == null || !_rules.CanUse(actor, ability))
				return;

			_selectedAbility.Value = _selectedAbility.Value == ability ? null : ability;
			SetHoveredTarget(null);
			RefreshTargetableUnits();
		}

		public void Skip()
		{
			BattleUnit actor = _awaitingActor.Value;

			if (actor != null)
				Submit(BattleCommand.Skip(actor));
		}

		public void HoverTarget(BattleUnit unit)
		{
			if (IsSelectableTarget(unit))
				SetHoveredTarget(unit);
		}

		public void UnhoverTarget(BattleUnit unit)
		{
			if (_hoveredTarget == unit)
				SetHoveredTarget(null);
		}

		public void SelectTarget(BattleUnit unit)
		{
			if (IsSelectableTarget(unit))
				Submit(new BattleCommand(_awaitingActor.Value, _selectedAbility.Value, unit));
		}

		public void InspectUnit(BattleUnit unit)
		{
			if (_selectedAbility.Value == null)
				_inspectedUnit.Value = unit;
		}

		public void UninspectUnit(BattleUnit unit)
		{
			if (_inspectedUnit.Value == unit)
				_inspectedUnit.Value = null;
		}

		public void CancelSelection()
		{
			_selectedAbility.Value = null;
			SetHoveredTarget(null);
			RefreshTargetableUnits();
		}

		private void SetHoveredTarget(BattleUnit unit)
		{
			_hoveredTarget = unit;
			_affectedUnits.Value = unit != null
				? _rules.GetAffectedUnits(_awaitingActor.Value, _selectedAbility.Value, unit)
				: Array.Empty<BattleUnit>();
		}

		private bool IsSelectableTarget(BattleUnit unit)
		{
			BattleUnit actor = _awaitingActor.Value;
			AbilityConfig ability = _selectedAbility.Value;

			return actor != null && ability != null && _rules.GetTargets(actor, ability).Contains(unit);
		}

		private void RefreshTargetableUnits()
		{
			BattleUnit actor = _awaitingActor.Value;
			AbilityConfig ability = _previewedAbility != null ? _previewedAbility : _selectedAbility.Value;

			if (actor == null || ability == null || !_rules.CanUse(actor, ability))
				_targetableUnits.Value = Array.Empty<BattleUnit>();
			else
				_targetableUnits.Value = _rules.GetTargets(actor, ability);
		}

		private void Submit(BattleCommand command)
		{
			UniTaskCompletionSource<BattleCommand> commandSource = _commandSource;

			_previewedAbility = null;
			_selectedAbility.Value = null;
			SetHoveredTarget(null);
			_awaitingActor.Value = null;
			_commandSource = null;
			RefreshTargetableUnits();

			commandSource.TrySetResult(command);
		}
	}
}
