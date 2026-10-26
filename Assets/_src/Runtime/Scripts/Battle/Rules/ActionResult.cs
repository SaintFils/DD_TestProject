namespace DDTest.Battle
{
	public readonly struct ActionResult
	{
		public BattleUnit Actor { get; }

		public AbilityConfig Ability { get; }

		public BattleUnit Target { get; }

		public ActionOutcome Outcome { get; }

		public int Amount { get; }

		public float HitChance { get; }


		private ActionResult(BattleUnit actor, AbilityConfig ability, BattleUnit target, ActionOutcome outcome, int amount, float hitChance)
		{
			Actor = actor;
			Ability = ability;
			Target = target;
			Outcome = outcome;
			Amount = amount;
			HitChance = hitChance;
		}


		public static ActionResult NoEffect(BattleUnit actor, AbilityConfig ability, BattleUnit target)
			=> new(actor, ability, target, ActionOutcome.NoEffect, 0, 1f);

		public static ActionResult Hit(BattleUnit actor, AbilityConfig ability, BattleUnit target, int amount, float hitChance)
			=> new(actor, ability, target, ActionOutcome.Hit, amount, hitChance);

		public static ActionResult Missed(BattleUnit actor, AbilityConfig ability, BattleUnit target, float hitChance)
			=> new(actor, ability, target, ActionOutcome.Missed, 0, hitChance);

		public static ActionResult Healed(BattleUnit actor, AbilityConfig ability, BattleUnit target, int amount)
			=> new(actor, ability, target, ActionOutcome.Healed, amount, 1f);
	}
}
