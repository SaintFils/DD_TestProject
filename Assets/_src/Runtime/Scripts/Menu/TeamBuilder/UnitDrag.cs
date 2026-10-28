using DDTest.Battle;
using R3;

namespace DDTest.Menu
{
	public sealed class UnitDrag
	{
		private readonly ReactiveProperty<UnitConfig> _unit = new();


		public ReadOnlyReactiveProperty<UnitConfig> Unit
			=> _unit;


		public void Begin(UnitConfig unit)
			=> _unit.Value = unit;

		public void End()
			=> _unit.Value = null;
	}
}
