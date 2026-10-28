using System;
using System.Collections.Generic;

namespace DDTest.Battle
{
	public readonly struct ActionResult
	{
		public BattleUnit Actor { get; }

		public AbilityConfig Ability { get; }

		public BattleUnit Target { get; }

		public IReadOnlyList<ActionEffect> Effects { get; }

		public bool IsSkip
			=> Ability == null;


		public ActionResult(BattleUnit actor, AbilityConfig ability, BattleUnit target, IReadOnlyList<ActionEffect> effects)
		{
			Actor = actor;
			Ability = ability;
			Target = target;
			Effects = effects;
		}


		public static ActionResult Skip(BattleUnit actor)
			=> new(actor, null, null, Array.Empty<ActionEffect>());
	}
}
