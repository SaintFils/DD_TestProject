using System.Linq;
using DDTest.Battle;
using DDTest.Infrastructure;

namespace DDTest.Tests
{
	public sealed class TestBattle
	{
		public BattleConfig Config { get; }

		public BattleState State { get; }

		public BattleRules Rules { get; }


		public TestBattle(BattleConfig config, IRandom random = null)
		{
			random ??= new StubRandom();

			Config = config;
			State = new BattleState(config, random);
			Rules = new BattleRules(State, config, random);
		}


		public BattleUnit Left(int slotIndex)
			=> State.LeftSide.Units.Single(unit => unit.SlotIndex == slotIndex);

		public BattleUnit Right(int slotIndex)
			=> State.RightSide.Units.Single(unit => unit.SlotIndex == slotIndex);

		public void Kill(BattleUnit unit)
			=> unit.TakeDamage(unit.MaxHp);

		public BattleLoop CreateLoop(ISideController left, ISideController right, IBattlePresenter presenter)
			=> new(State, Rules, new SideControllers(left, right), presenter);
	}
}
