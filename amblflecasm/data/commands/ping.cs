using Discord.Interactions;

namespace amblflecasm.data.commands
{
	public class ping : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("ping", "Pong!", false, RunMode.Async)]
		public async Task run()
		{
			string box = amblflecasm.util.Box("Latency", new string[]
			{
				string.Format("Gateway Latency     :    {0} ms", this.Context.Client.Latency),
				string.Format("Interaction Latency :    {0} ms", DateTimeOffset.UtcNow.Millisecond - this.Context.Client.Latency)
			});

			await this.RespondAsync(box);
		}
	}
}
