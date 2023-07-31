using Discord.Interactions;
using Newtonsoft.Json;

namespace amblflecasm.data.commands
{
	public class joke : InteractionModuleBase<SocketInteractionContext>
	{
		private static string formatString = "{0}\n{1}";

		[RateLimit(5)]
		[SlashCommand("joke", "Tell a funny joke", false, RunMode.Async)]
		public async Task run()
		{
			await this.RespondAsync("Writing a joke");

			try
			{
				dynamic? json;

				using (HttpClient httpClient = new HttpClient())
				{
					string data = await httpClient.GetStringAsync("https://v2.jokeapi.dev/joke/Any");

					json = JsonConvert.DeserializeObject(data);
					if (json == null || json.error != false)
					{
						await this.ModifyOriginalResponseAsync(message => message.Content = "Unable to get joke (Probably rate limited)");
						return;
					}
				}

				await this.ModifyOriginalResponseAsync(message => message.Content = string.Format(formatString, json.setup, json.delivery));
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Sorry, my pen broke");
			}
		}
	}
}
