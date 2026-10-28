using DDTest.Battle;
using DDTest.Infrastructure;
using NaughtyAttributes;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DDTest.Menu
{
	public class TeamBuilderView : MonoBehaviour
	{
		[Inject]
		private readonly TeamSelection __selection;

		[Inject]
		private readonly TeamBuilderConfig __config;

		[Inject]
		private readonly IRandom __random;

		[Inject]
		private readonly RosterCardView.Factory __rosterCardFactory;


		[SerializeField, Required]
		private RectTransform _rosterGrid;

		[SerializeField, Required]
		private UnitInfoView _unitInfo;

		[SerializeField, Required]
		private Button _backButton;

		[SerializeField, Required]
		private Button _randomButton;

		[SerializeField, Required]
		private Button _playButton;


		public Observable<Unit> BackClicked
			=> _backButton.OnClickAsObservable();

		public Observable<Unit> PlayClicked
			=> _playButton.OnClickAsObservable();


		private void Awake()
		{
			foreach (UnitConfig unit in __config.Roster)
			{
				RosterCardView card = __rosterCardFactory.Create(unit);
				card.transform.SetParent(_rosterGrid, false);

				card.Clicked
					.Subscribe(_unitInfo.Show)
					.AddTo(this);
			}

			_randomButton.OnClickAsObservable()
				.Subscribe(_ => __selection.FillEmpty(__config.Roster, __random))
				.AddTo(this);

			__selection.Changed
				.Subscribe(_ => RefreshButtons())
				.AddTo(this);

			RefreshButtons();
		}


		private void RefreshButtons()
		{
			_playButton.interactable = __selection.CanPlay;
			_randomButton.interactable = __selection.HasEmptySlots && __config.Roster.Count > 0;
		}
	}
}
