namespace DDTest.Battle
{
	public readonly struct ActionEffect
	{
		public BattleUnit Target { get; }

		public ActionOutcome Outcome { get; }

		public int Amount { get; }

		public float HitChance { get; }


		private ActionEffect(BattleUnit target, ActionOutcome outcome, int amount, float hitChance)
		{
			Target = target;
			Outcome = outcome;
			Amount = amount;
			HitChance = hitChance;
		}


		public static ActionEffect Hit(BattleUnit target, int amount, float hitChance)
			=> new(target, ActionOutcome.Hit, amount, hitChance);

		public static ActionEffect Missed(BattleUnit target, float hitChance)
			=> new(target, ActionOutcome.Missed, 0, hitChance);

		public static ActionEffect Healed(BattleUnit target, int amount)
			=> new(target, ActionOutcome.Healed, amount, 1f);
	}
}
