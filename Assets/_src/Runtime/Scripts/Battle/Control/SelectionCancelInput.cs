using UnityEngine.InputSystem;
using Zenject;

namespace DDTest.Battle
{
	public sealed class SelectionCancelInput : ITickable
	{
		private readonly PlayerController _player;


		public SelectionCancelInput(PlayerController player)
		{
			_player = player;
		}


		public void Tick()
		{
			bool isRightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
			bool isEscape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

			if (isRightClick || isEscape)
				_player.CancelSelection();
		}
	}
}
