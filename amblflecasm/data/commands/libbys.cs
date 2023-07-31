using Discord;
using Discord.Interactions;
using Renci.SshNet;

namespace amblflecasm.data.commands
{
	[Group("libbys", "libbys commands")]
	public class libbys : InteractionModuleBase<SocketInteractionContext>
	{
		[RateLimit(600, 1, RateLimit.RateLimitType.Global)]
		[SlashCommand("restart", "Restarts the Libby's sandbox server", false, RunMode.Async)]
		public async Task restart()
		{
			await this.RespondAsync("Attempting server restart");

			try
			{
				string staffString = amblflecasm.config.GetString("roles", "libbys_staff");
				ulong staffID = ulong.Parse(staffString);

				ulong? foundStaffID = ((IGuildUser)this.Context.User).RoleIds.First(id => id == staffID); // This should always throw an error if it's not found, but a check is still in place just in case

				if (foundStaffID == null)
					throw new Exception();
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "You do not have permission to run this command!");
				return;
			}

			try
			{
				SshClient client = new SshClient(amblflecasm.config.GetString("ips", "libbys"), "gmodserver", amblflecasm.config.GetString("tokens", "libbys"));
				client.Connect();

				SshCommand command = client.CreateCommand("./gmodserver restart");
				command.Execute();

				await this.ModifyOriginalResponseAsync(message => message.Content = "Restart command sent!");

				command.Dispose();

				client.Disconnect();
				client.Dispose();
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to send command");
			}
		}
	}
}
