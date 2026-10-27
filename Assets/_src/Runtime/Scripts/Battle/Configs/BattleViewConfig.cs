using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Battle View Config", fileName = "battle_view_config")]
	public partial class BattleViewConfig : ScriptableObject
	{
		[Header("Highlight")]
		[SerializeField, GenPublicGetter]
		private Color _currentTurnColor = new(1f, 0.85f, 0.3f, 1f);

		[SerializeField, GenPublicGetter]
		private Color _selectionColor = new(0.3f, 0.9f, 1f, 1f);

		[SerializeField, Min(0f), GenPublicGetter]
		private float _highlightFadeDuration = 0.15f;

		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _actionTargetScale = 1.1f;

		[Header("Death")]
		[SerializeField, Range(0f, 1f), GenPublicGetter]
		private float _deadAlpha = 0.3f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _deathDuration = 0.5f;

		[Header("Attack")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _lungeDistance = 60f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _lungeDuration = 0.15f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _recoilDistance = 20f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _recoilDuration = 0.2f;

		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _castScale = 1.15f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _castDuration = 0.3f;

		[Header("Projectile")]
		[SerializeField, Range(1f, 50f)]
		private float _projectileSpeed = 25f;

		[SerializeField, GenPublicGetter]
		private Vector2 _projectileMissOffset = new(0f, 80f);

		[SerializeField, Min(0f), GenPublicGetter]
		private float _projectileMissDistance = 300f;

		[Header("Reaction")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hitShakeStrength = 15f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _hitDuration = 0.3f;

		[SerializeField, GenPublicGetter]
		private Color _hitColor = new(1f, 0.35f, 0.35f, 1f);

		[SerializeField, Min(0f), GenPublicGetter]
		private float _dodgeDistance = 40f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _dodgeDuration = 0.25f;

		[SerializeField, GenPublicGetter]
		private Color _healColor = new(0.4f, 1f, 0.4f, 1f);

		[SerializeField, Min(0f), GenPublicGetter]
		private float _healDuration = 0.4f;

		[Header("Hp Bar")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hpFastDuration = 0.2f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _hpSlowDelay = 0.3f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _hpSlowDuration = 0.6f;

		[Header("Initiative Queue")]
		[SerializeField, GenPublicGetter]
		private Color _leftSideColor = new(0.3f, 0.6f, 1f, 1f);

		[SerializeField, GenPublicGetter]
		private Color _rightSideColor = new(1f, 0.35f, 0.3f, 1f);

		[SerializeField, GenPublicGetter]
		private Color _actedCardColor = new(0.4f, 0.4f, 0.4f, 1f);

		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _currentCardScale = 1.2f;

		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _cardDeathScale = 1.15f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardDeathPopDuration = 0.1f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardDeathDrop = 60f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardDeathDuration = 0.4f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardCollapseDuration = 0.25f;

		[Header("Round Banner")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _roundBannerFadeDuration = 0.25f;

		[SerializeField, Min(0f), GenPublicGetter]
		private float _roundBannerHoldDuration = 0.6f;
		
		
		public float ProjectileSpeed
			=> _projectileSpeed * 100f;
	}
}
