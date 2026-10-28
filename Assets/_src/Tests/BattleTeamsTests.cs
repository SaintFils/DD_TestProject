using DDTest.Battle;
using NUnit.Framework;

namespace DDTest.Tests
{
	public class BattleTeamsTests
	{
		private ConfigFactory _configs;


		[SetUp]
		public void SetUp()
			=> _configs = new ConfigFactory();

		[TearDown]
		public void TearDown()
			=> _configs.Dispose();

		[Test]
		public void Constructor_CopiesSlots_SoLaterChangesDoNotAffectTeams()
		{
			UnitConfig unit = _configs.Unit();
			UnitConfig[] left = { unit, null };
			UnitConfig[] right = { null, unit };

			BattleTeams teams = new(left, right);
			left[0] = null;
			right[1] = null;

			Assert.That(teams.Left, Is.EqualTo(new[] { unit, null }));
			Assert.That(teams.Right, Is.EqualTo(new[] { null, unit }));
		}
	}
}
