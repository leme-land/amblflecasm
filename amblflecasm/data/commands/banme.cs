using Discord;
using Discord.Interactions;

namespace amblflecasm.data.commands
{
	public class banme : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("banme", "Gain free administrator permission", false, RunMode.Async)]
		public async Task run()
		{
			IGuildUser guildUser = (IGuildUser)this.Context.User;

			if (guildUser.GuildPermissions.Administrator)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			try
			{
				await guildUser.BanAsync(0, "Retard banned himself");
				await this.RespondAsync("Here you go!");
			}
			catch (Exception)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
			}
		}
	}
}
