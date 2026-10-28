using UnityEngine.InputSystem;
using Zenject;

namespace DDTest.Battle
{
	public sealed class BattleInput : ITickable
	{
		private readonly BattleState _battle;
		private readonly PlayerController _player;
		private readonly BattlePause _pause;


		public BattleInput(BattleState battle, PlayerController player, BattlePause pause)
		{
			_battle = battle;
			_player = player;
			_pause = pause;
		}


		public void Tick()
		{
			if (_battle.IsFinished)
				return;

			bool isRightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
			bool isEscape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

			if (_pause.IsPaused.CurrentValue)
			{
				if (isEscape)
					_pause.Resume();

				return;
			}

			if (isRightClick || (isEscape && _player.SelectedAbility.CurrentValue != null))
			{
				_player.CancelSelection();
			}
			else if (isEscape)
			{
				_pause.Pause();
			}
		}
	}
}
