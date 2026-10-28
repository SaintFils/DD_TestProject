using NaughtyAttributes;
using UnityEngine;
using Valhalla.PublicGetter.Generators;

namespace DDTest.Battle
{
	[CreateAssetMenu(menuName = "DDTest/Battle/Battle View Config", fileName = "battle_view_config")]
	public partial class BattleViewConfig : ScriptableObject
	{
		[Foldout("Highlight")]
		[SerializeField, GenPublicGetter]
		private Color _activityColor = new(1f, 0.85f, 0.3f, 1f);

		[Foldout("Highlight")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _highlightFadeDuration = 0.15f;

		[Foldout("Focus")]
		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _participantScale = 1.2f;

		[Foldout("Focus")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _participantScaleDuration = 0.15f;

		[Foldout("Death")]
		[SerializeField, Range(0f, 1f), GenPublicGetter]
		private float _deadAlpha = 0.3f;

		[Foldout("Death")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _deathDuration = 0.5f;

		[Foldout("Attack")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _lungeDistance = 60f;

		[Foldout("Attack")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _lungeDuration = 0.15f;

		[Foldout("Attack")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _recoilDistance = 20f;

		[Foldout("Attack")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _recoilDuration = 0.2f;

		[Foldout("Attack")]
		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _castScale = 1.15f;

		[Foldout("Attack")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _castDuration = 0.3f;

		[Foldout("Projectile")]
		[SerializeField, Range(1f, 50f)]
		private float _projectileSpeed = 25f;
		
		[Foldout("Projectile")]
		[SerializeField, GenPublicGetter]
		private Vector2 _projectileMissOffset = new(0f, 80f);

		[Foldout("Projectile")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _projectileMissDistance = 300f;

		[Foldout("Reaction")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hitShakeStrength = 15f;

		[Foldout("Reaction")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hitDuration = 0.3f;

		[Foldout("Reaction")]
		[SerializeField, GenPublicGetter]
		private Color _hitColor = new(1f, 0.35f, 0.35f, 1f);

		[Foldout("Reaction")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _dodgeDistance = 40f;

		[Foldout("Reaction")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _dodgeDuration = 0.25f;

		[Foldout("Reaction")]
		[SerializeField, GenPublicGetter]
		private Color _healColor = new(0.4f, 1f, 0.4f, 1f);

		[Foldout("Reaction")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _healDuration = 0.4f;

		[Foldout("Hp Bar")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hpFastDuration = 0.2f;

		[Foldout("Hp Bar")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hpSlowDelay = 0.3f;

		[Foldout("Hp Bar")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _hpSlowDuration = 0.6f;

		[Foldout("Initiative Queue")]
		[SerializeField, GenPublicGetter]
		private Color _leftSideColor = new(0.3f, 0.6f, 1f, 1f);

		[Foldout("Initiative Queue")]
		[SerializeField, GenPublicGetter]
		private Color _rightSideColor = new(1f, 0.35f, 0.3f, 1f);

		[Foldout("Initiative Queue")]
		[SerializeField, GenPublicGetter]
		private Color _actedCardColor = new(0.4f, 0.4f, 0.4f, 1f);

		[Foldout("Initiative Queue")]
		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _currentCardScale = 1.2f;

		[Foldout("Initiative Queue")]
		[SerializeField, Range(1f, 1.5f), GenPublicGetter]
		private float _cardDeathScale = 1.15f;

		[Foldout("Initiative Queue")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardDeathPopDuration = 0.1f;

		[Foldout("Initiative Queue")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardDeathDrop = 60f;

		[Foldout("Initiative Queue")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardDeathDuration = 0.4f;

		[Foldout("Initiative Queue")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _cardCollapseDuration = 0.25f;

		[Foldout("Round Banner")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _battleStartDelay = 0.5f;

		[Foldout("Round Banner")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _roundBannerFadeDuration = 0.25f;

		[Foldout("Round Banner")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _roundBannerHoldDuration = 0.6f;

		[Foldout("Battle Log")]
		[SerializeField, GenPublicGetter]
		private Color _logMutedColor = new(0.6f, 0.6f, 0.6f, 1f);

		[Foldout("Battle Log")]
		[SerializeField, Min(1), GenPublicGetter]
		private int _logMaxLines = 200;

		[Foldout("Result")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _resultFadeDuration = 0.4f;

		[Foldout("Pause")]
		[SerializeField, Min(0f), GenPublicGetter]
		private float _pauseFadeDuration = 0.2f;


		public float ProjectileSpeed
			=> _projectileSpeed * 100f;


		public Color GetSideColor(BattleSideId side)
			=> side == BattleSideId.Left ? _leftSideColor : _rightSideColor;
	}
}
