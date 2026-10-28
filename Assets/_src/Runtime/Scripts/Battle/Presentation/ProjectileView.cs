using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Battle
{
	public class ProjectileView : MonoBehaviour
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;


		[SerializeField, Required]
		private Image _image;


		private Color _color;
		private Vector2 _direction;


		private void Awake()
		{
			_color = _image.color;
			_image.enabled = false;
		}


		public UniTask FlyAsync(Vector2 start, Vector2 end, float duration, CancellationToken cancellationToken)
		{
			Vector2 path = end - start;

			_direction = path.normalized;
			transform.localPosition = start;
			transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(path.y, path.x) * Mathf.Rad2Deg);
			_image.color = _color;
			_image.enabled = true;

			return transform.DOLocalMove(end, duration)
				.SetEase(Ease.Linear)
				.SetLink(gameObject)
				.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}

		public async UniTask FlyAwayAsync(CancellationToken cancellationToken)
		{
			float duration = __viewConfig.ProjectileMissDistance / __viewConfig.ProjectileSpeed;
			Vector2 end = (Vector2)transform.localPosition + _direction * __viewConfig.ProjectileMissDistance;

			await DOTween.Sequence().Append(transform.DOLocalMove(end, duration).SetEase(Ease.Linear))
				.Join(_image.DOFade(0f, duration))
				.SetLink(gameObject)
				.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);

			Hide();
		}

		public void Hide()
			=> _image.enabled = false;


		public class Factory : PlaceholderFactory<ProjectileView>
		{
		}
	}
}
