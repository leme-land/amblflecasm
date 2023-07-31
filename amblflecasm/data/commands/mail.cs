using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace amblflecasm.data.commands
{
	public class mail : InteractionModuleBase<SocketInteractionContext>
	{
		private static readonly string formatString = "Message from {0}:";

		[RateLimit(300)]
		[SlashCommand("mail", "Send someone a message", false, RunMode.Async)]
		public async Task run(SocketUser target, string message, bool anonymous = false)
		{
			if (message.Length > 2000)
				message = message.Substring(0, 2000);

			await this.RespondAsync("Attempting to send message", null, false, true);

			try
			{
				string username;
				if (anonymous)
					username = "*Anonymous*";
				else
					username = this.Context.User.Username;

				await target.SendMessageAsync(string.Format(formatString, username));
				await target.SendMessageAsync(message);

				await this.ModifyOriginalResponseAsync(message => message.Content = "Message sent!");
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to send message!");
			}
		}
	}
}
