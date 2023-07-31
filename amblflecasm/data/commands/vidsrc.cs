using Discord.Interactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace amblflecasm.data.commands
{
	[Group("vidsrc", "https://vidsrc.me")]
	public class vidsrc : InteractionModuleBase<SocketInteractionContext>
	{
		private static readonly int MAX_PER_PAGE = 10; // Need to split it up to avoid character limit
		private static string pageFormatString = "https://vidsrc.me/movies/latest/page-{0}.json";

		private async Task<int> GetPageCount()
		{
			dynamic? json = null;

			using (HttpClient httpClient = new HttpClient())
			{
				string data = await httpClient.GetStringAsync(string.Format(pageFormatString, 1));

				json = JsonConvert.DeserializeObject(data);
			}

			if (json?.pages != null)
				return int.Parse(Convert.ToString(json.pages));

			return -1;
		}

		private async Task<dynamic?> GetPage(int page)
		{
			dynamic? json = null;

			using (HttpClient httpClient = new HttpClient())
			{
				string data = await httpClient.GetStringAsync(string.Format(pageFormatString, page));

				json = JsonConvert.DeserializeObject(data);
			}

			return json;
		}

		private static readonly string pageTitleFormatString = "VIdSrc Page #{0}";

		[RateLimit(5)]
		[SlashCommand("browse", "Browse to a certain page", false, RunMode.Async)]
		public async Task browse(int page)
		{
			int maxPages = await GetPageCount();
			if (page < 1 || page > maxPages)
			{
				await this.RespondAsync(string.Format("Invalid page! Max page is `{0}`", maxPages));
				return;
			}

			await this.RespondAsync("Generating pages"); // Have some patience, friend

			dynamic? pageData = await GetPage(page);
			if (pageData != null && pageData?.result != null)
			{
				try
				{
					JArray pageResults = (JArray)pageData.result;

					Dictionary<string, string> data = new Dictionary<string, string>();
					foreach (JObject entry in pageResults)
						data.Add(entry["imdb_id"].ToString(), entry["title"].ToString());

					List<string> boxes = amblflecasm.util.SplitBox(string.Format(pageTitleFormatString, page), data, 10, true);

					if (boxes.Count == 1)
						await this.ModifyOriginalResponseAsync(message => message.Content = boxes[0]);
					else
					{
						await this.DeleteOriginalResponseAsync();

						foreach (string box in boxes)
							await this.Context.Channel.SendMessageAsync(box);
					}
				}
				catch (Exception)
				{
					await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to generate pages");
				}

			}
			else
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get page data");
		}

		[RateLimit(1)]
		[Group("get", "'https://vidsrc.me' get")]
		public class vidsrc_get : InteractionModuleBase<SocketInteractionContext>
		{
			private static string imdbTestFormat = "https://www.imdb.com/title/{0}/";
			private static string tmdbTestFormat = "https://www.themoviedb.org/movie/{0}/";
			private static string dbOutputFormat = "Here you go! <https://v2.vidsrc.me/embed/{0}/>";

			[SlashCommand("imdb", "Get the link from an IMDB ID", false, RunMode.Async)]
			public async Task getIMDB(string id)
			{
				id = id.Trim();

				using (HttpClient httpClient = new HttpClient())
				{
					HttpResponseMessage response = await httpClient.GetAsync(string.Format(imdbTestFormat, id));

					if (response.IsSuccessStatusCode)
						await this.RespondAsync(string.Format(dbOutputFormat, id));
					else
						await this.RespondAsync("Invalid ID!");
				}
			}

			[SlashCommand("tmdb", "Get the link from an TMDB ID", false, RunMode.Async)]
			public async Task getTMDB(string id)
			{
				id = id.Trim();

				using (HttpClient httpClient = new HttpClient())
				{
					HttpResponseMessage response = await httpClient.GetAsync(string.Format(tmdbTestFormat, id));

					if (response.IsSuccessStatusCode)
						await this.RespondAsync(string.Format(dbOutputFormat, id));
					else
						await this.RespondAsync("Invalid ID!");
				}
			}
		}
	}
}
