using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace amblflecasm.data.commands
{
	public class kick : InteractionModuleBase<SocketInteractionContext>
	{
		private static string formatString = "Kicked `{0}` for `{1}`";

		[SlashCommand("kick", "Kick someone", false, RunMode.Async)]
		public async Task run(SocketUser target, string reason = "Get owned")
		{
			IGuildUser guildUser = (IGuildUser)this.Context.User;
			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.KickMembers)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			IGuildUser guildTarget = (IGuildUser)target;
			if (guildTarget == null)
			{
				await this.RespondAsync("Invalid user!");
				return;
			}

			if (guildTarget.GuildPermissions.Administrator)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			try
			{
				await guildTarget.KickAsync(reason);
				await this.RespondAsync(string.Format(formatString, target.Username, reason));
			}
			catch (Exception)
			{
				await this.RespondAsync("I epic failed");
			}
		}
	}
}
