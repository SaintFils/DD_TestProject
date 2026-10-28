namespace DDTest.Battle
{
	public readonly struct BattleCommand
	{
		public BattleUnit Actor { get; }

		public AbilityConfig Ability { get; }

		public BattleUnit Target { get; }

		public bool IsSkip
			=> Ability == null;


		public BattleCommand(BattleUnit actor, AbilityConfig ability, BattleUnit target)
		{
			Actor = actor;
			Ability = ability;
			Target = target;
		}


		public static BattleCommand Skip(BattleUnit actor)
			=> new(actor, null, null);
	}
}
