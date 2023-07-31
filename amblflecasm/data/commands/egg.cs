using Discord.Interactions;
using Discord.WebSocket;

namespace amblflecasm.data.commands
{
	public class egg : InteractionModuleBase<SocketInteractionContext>
	{
		private static string formatString = "<@{0}> :egg: :egg: :egg:";

		[SlashCommand("egg", "Egg someone", false, RunMode.Async)]
		public async Task run(SocketUser target)
		{
			await this.RespondAsync(string.Format(formatString, target.Id));
		}
	}
}
