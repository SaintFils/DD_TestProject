namespace DDTest.Battle
{
	public sealed class BattleLaunch
	{
		public int? Seed { get; set; }

		public BattleTeams Teams { get; set; }

		public bool IsLeftAi { get; set; }

		public bool IsRightAi { get; set; } = true;
	}
}
