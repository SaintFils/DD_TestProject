using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace DDTest.Battle
{
	public class ProjectilePoolView : MonoBehaviour
	{
		[Inject]
		private readonly BattleViewConfig __viewConfig;

		[Inject]
		private readonly ProjectileView.Factory __projectileFactory;


		private readonly List<ProjectileView> _projectiles = new();
		private readonly List<(ProjectileView Projectile, bool IsMiss)> _launched = new();


		private void Awake()
		{
			for (int index = 0; index < TeamConfig.SlotCount; index++)
			{
				ProjectileView projectile = __projectileFactory.Create();
				projectile.transform.SetParent(transform, false);
				_projectiles.Add(projectile);
			}
		}


		public UniTask FireAsync(Vector3 from, IReadOnlyList<(Vector3 Target, bool IsMiss)> shots, CancellationToken cancellationToken)
		{
			if (shots.Count > _projectiles.Count)
				Debug.LogWarning($"ProjectilePoolView | {shots.Count} shots, only {_projectiles.Count} projectiles", this);

			int count = Mathf.Min(shots.Count, _projectiles.Count);
			Vector2 start = transform.InverseTransformPoint(from);
			
			List<Vector2> ends = shots
				.Take(count)
				.Select(shot => (Vector2)transform.InverseTransformPoint(shot.Target) + (shot.IsMiss ? __viewConfig.ProjectileMissOffset : Vector2.zero))
				.ToList();
			
			float duration = ends.Max(end => Vector2.Distance(start, end)) / __viewConfig.ProjectileSpeed;
			List<UniTask> flights = new();

			_launched.Clear();

			for (int index = 0; index < count; index++)
			{
				_launched.Add((_projectiles[index], shots[index].IsMiss));
				flights.Add(_projectiles[index].FlyAsync(start, ends[index], duration, cancellationToken));
			}

			return UniTask.WhenAll(flights);
		}

		public UniTask FinishAsync(CancellationToken cancellationToken)
		{
			List<UniTask> flights = new();

			foreach ((ProjectileView projectile, bool isMiss) in _launched)
			{
				if (isMiss)
					flights.Add(projectile.FlyAwayAsync(cancellationToken));
				else
					projectile.Hide();
			}

			_launched.Clear();

			return UniTask.WhenAll(flights);
		}
	}
}
