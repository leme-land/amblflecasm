using Discord.Interactions;
using Newtonsoft.Json;

namespace amblflecasm.data.commands
{
	public class ocr : InteractionModuleBase<SocketInteractionContext>
	{
		private static string urlFormatString = "https://api.ocr.space/parse/imageurl?apikey={0}&url={1}";

		[RateLimit(5)]
		[SlashCommand("ocr", "Optical Character Recognition", false, RunMode.Async)]
		public async Task run(string imageURL)
		{
			await this.RespondAsync("Parsing text");

			try
			{
				dynamic? json;

				using (HttpClient httpClient = new HttpClient())
				{
					string data = await httpClient.GetStringAsync(string.Format(urlFormatString, amblflecasm.config.GetString("tokens", "ocr"), imageURL.Trim()));

					json = JsonConvert.DeserializeObject(data);
					if (json == null || json.ParsedResults?[0]?.ParsedText == null)
						throw new Exception();
				}

				await this.ModifyOriginalResponseAsync(message => message.Content = Convert.ToString(json.ParsedResults[0].ParsedText));
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to parse text");
			}
		}
	}
}
