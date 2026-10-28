namespace DDTest.Battle
{
	public sealed class SideControllers
	{
		private readonly ISideController _left;
		private readonly ISideController _right;


		public SideControllers(ISideController left, ISideController right)
		{
			_left = left;
			_right = right;
		}


		public ISideController Get(BattleSideId side)
			=> side == BattleSideId.Left ? _left : _right;
	}
}
