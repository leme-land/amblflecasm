using Discord.Interactions;

namespace amblflecasm.data.commands
{
	public class dice : InteractionModuleBase<SocketInteractionContext>
	{
		private static Random? rng;
		private static readonly string formatString = "Rolled {0} {1} and got {2}";

		[SlashCommand("dice", "Roll some dice", false, RunMode.Async)]
		public async Task run(byte amount = 1)
		{
			rng = new Random(Guid.NewGuid().GetHashCode()); // Change the seed every time

			uint result = 0;
			for (byte i = 1; i <= amount; i++)
				result += (uint)rng.Next(1, 6);

			await this.RespondAsync(string.Format(formatString, amount, amblflecasm.util.Plural("di", amount, "e", "ce"), result));
		}
	}
}
