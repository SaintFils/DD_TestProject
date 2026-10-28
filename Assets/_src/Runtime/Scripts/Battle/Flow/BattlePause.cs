using System;
using R3;
using UnityEngine;

namespace DDTest.Battle
{
	public sealed class BattlePause : IDisposable
	{
		private readonly BattleState _battle;
		private readonly PlayerController _player;
		private readonly ReactiveProperty<bool> _isPaused = new(false);


		public ReadOnlyReactiveProperty<bool> IsPaused
			=> _isPaused;


		public BattlePause(BattleState battle, PlayerController player)
		{
			_battle = battle;
			_player = player;
		}


		public void Pause()
		{
			if (_isPaused.Value || _battle.IsFinished)
				return;

			_player.CancelSelection();
			_isPaused.Value = true;
			Time.timeScale = 0f;
		}

		public void Resume()
		{
			if (!_isPaused.Value)
				return;

			_isPaused.Value = false;
			Time.timeScale = 1f;
		}

		public void Dispose()
			=> Time.timeScale = 1f;
	}
}
