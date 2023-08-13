using Discord.Interactions;
using Newtonsoft.Json;

namespace amblflecasm.data.commands
{
	public class joke : InteractionModuleBase<SocketInteractionContext>
	{
		public enum JokeType
		{
			Any,
			Programming,
			Miscellaneous,
			Dark,
			Pun,
			Spooky,
			Christmas
		};

		private static string formatString = "{0}\n{1}";
		private static string urlFormatString = "https://v2.jokeapi.dev/joke/{0}";

		public string GetURL(JokeType jokeType)
		{
			string tail = string.Empty;

			switch (jokeType)
			{
				case JokeType.Programming:
					tail = "programming";
					break;

				case JokeType.Miscellaneous:
					tail = "miscellaneous";
					break;

				case JokeType.Dark:
					tail = "dark";
					break;

				case JokeType.Pun:
					tail = "pun";
					break;

				case JokeType.Spooky:
					tail = "spooky";
					break;

				case JokeType.Christmas:
					tail = "christmas";
					break;

				case JokeType.Any:
				default:
					tail = "any";
					break;
			}

			return string.Format(urlFormatString, tail);
		}

		[RateLimit(10)]
		[SlashCommand("joke", "Tell a funny joke", false, RunMode.Async)]
		public async Task run(JokeType jokeType = JokeType.Any)
		{
			await this.RespondAsync("Writing a joke");

			try
			{
				string url = GetURL(jokeType);

				dynamic? json;

				using (HttpClient httpClient = new HttpClient())
				{
					string data = await httpClient.GetStringAsync(url);

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
