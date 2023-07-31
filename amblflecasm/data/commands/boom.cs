using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace amblflecasm.data.commands
{
	public class boom : InteractionModuleBase<SocketInteractionContext>
	{
		private static readonly string initialFormat = "Booming {0} {1} {2}";
		private static readonly string finalFormat = "Boomed {0} {1} {2}";

		[SlashCommand("boom", "Vine BOOM", false, RunMode.Async)]
		public async Task run(SocketUser target, byte amount = 10)
		{
			if (!amblflecasm.util.IsUser(this.Context.User, "leme") || amount < 1)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			await this.RespondAsync(string.Format(initialFormat, target.Username, amount, amblflecasm.util.Plural("time", amount)));

			byte booms = 0;
			try
			{
				for (byte i = 1; i <= amount; i++)
				{
					await target.SendMessageAsync("https://tenor.com/view/vineboom-ilybeeduo-gif-23126674");
					booms++;

					await Task.Delay(100);
				}
			}
			catch (Exception) { }

			await this.ModifyOriginalResponseAsync(message => message.Content = string.Format(finalFormat, target.Username, booms, amblflecasm.util.Plural("time", booms)));
		}
	}
}
