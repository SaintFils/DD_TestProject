namespace DDTest.Battle
{
	public readonly struct BattleCommand
	{
		public BattleUnit Actor { get; }

		public AbilityConfig Ability { get; }

		public BattleUnit Target { get; }


		public BattleCommand(BattleUnit actor, AbilityConfig ability, BattleUnit target)
		{
			Actor = actor;
			Ability = ability;
			Target = target;
		}
	}
}
