using Discord.Interactions;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace amblflecasm.data.commands
{
	public class steam : InteractionModuleBase<SocketInteractionContext>
	{
		public enum SteamIDFormat
		{
			Steam2,
			Steam32,
			Steam64,
			CustomURL
		}

		private static class SteamIDRegex // https://github.com/NachoReplay/SteamID-NET
		{
			public const string Steam2Regex = "^STEAM_0:[0-1]:([0-9]{1,10})$";
			public const string Steam32Regex = "^U:1:([0-9]{1,10})$";
			public const string Steam64Regex = "^7656119([0-9]{10})$";
		}

		private static class SteamIDConverter
		{
			public static long Steam32ToSteam64(string input)
			{
				long steam32 = Convert.ToInt64(input.Substring(4));
				if (steam32 < 1L || !Regex.IsMatch("U:1:" + steam32.ToString((IFormatProvider)CultureInfo.InvariantCulture), SteamIDRegex.Steam32Regex))
					return 0;

				return steam32 + 76561197960265728L;
			}

			public static long Steam2ToSteam64(string accountID)
			{
				if (!Regex.IsMatch(accountID, SteamIDRegex.Steam2Regex))
					return 0;

				return 76561197960265728L + Convert.ToInt64(accountID.Substring(10)) * 2L + Convert.ToInt64(accountID.Substring(8, 1));
			}

			public static string Steam64ToSteam2(long steam64)
			{
				if (steam64 < 76561197960265729L || !Regex.IsMatch(steam64.ToString((IFormatProvider)CultureInfo.InvariantCulture), SteamIDRegex.Steam64Regex))
					return string.Empty;

				steam64 -= 76561197960265728L;

				long num = steam64 % 2L;
				steam64 -= num;

				string output = string.Format("STEAM_0:{0}:{1}", num, (steam64 / 2L));
				if (!Regex.IsMatch(output, SteamIDRegex.Steam2Regex))
					return string.Empty;

				return output;
			}
		}

		private static string idFormatString = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}";
		private static string vanityFormatString = "https://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}";

		[SlashCommand("steam", "Steam ID lookup", false, RunMode.Async)]
		public async Task run(string steamID, SteamIDFormat format)
		{
			await this.RespondAsync("Getting information");

			steamID = steamID.Trim();

			long longID = long.MaxValue;

			switch (format)
			{
				case SteamIDFormat.Steam2:
					if (Regex.IsMatch(steamID, SteamIDRegex.Steam2Regex))
						longID = SteamIDConverter.Steam2ToSteam64(steamID);

					break;

				case SteamIDFormat.Steam32:
					if (Regex.IsMatch(steamID, SteamIDRegex.Steam32Regex))
						longID = SteamIDConverter.Steam32ToSteam64(steamID);

					break;

				case SteamIDFormat.Steam64:
					if (Regex.IsMatch(steamID, SteamIDRegex.Steam64Regex) && long.TryParse(steamID, out longID))
						longID = long.Parse(steamID);

					break;

				case SteamIDFormat.CustomURL:
					if (steamID.ToLower().Contains("steamcommunity") && steamID.Length >= 30) // Assume entire URL
					{
						steamID = steamID.Substring(30);

						if (steamID.EndsWith("/") && steamID.Length >= 1)
							steamID = steamID.Substring(0, steamID.Length - 1);
					}

					using (HttpClient httpClient = new HttpClient())
					{
						string data = await httpClient.GetStringAsync(string.Format(vanityFormatString, amblflecasm.config.GetString("tokens", "steam"), steamID));

						dynamic? json = JsonConvert.DeserializeObject(data);
						if (json != null && json.response?.steamid != null)
							long.TryParse(Convert.ToString(json.response.steamid), out longID);

					}

					break;
			}

			if (longID == long.MaxValue)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Invalid SteamID");
				return;
			}

			try
			{
				dynamic? json;

				using (HttpClient httpClient = new HttpClient())
				{
					string data = await httpClient.GetStringAsync(string.Format(idFormatString, amblflecasm.config.GetString("tokens", "steam"), longID));

					json = JsonConvert.DeserializeObject(data);
					if (json == null)
						throw new Exception();

					if (json.response?.players == null || json.response?.players?[0] == null)
						throw new Exception();
				}

				json = json.response.players[0];

				string dateCreatedFinal = "Private"; // Private profiles make this null
				int dateCreated = int.MaxValue;
				if (json.timecreated != null && int.TryParse(json.timecreated.ToString(), out dateCreated))
					dateCreatedFinal = amblflecasm.util.GetTimestamp(dateCreated).ToString("MM/dd/yyyy");

				string box = amblflecasm.util.Box(SteamIDConverter.Steam64ToSteam2(longID), new string[]
				{
					string.Format("Username     : {0}", amblflecasm.util.SafeString(json.personaname)),
					string.Format("Real Name    : {0}", amblflecasm.util.SafeString(json.realname, "Unspecified")),
					string.Format("Country      : {0}", amblflecasm.util.SafeString(json.loccountrycode, "Unspecified")),
					string.Format("State        : {0}", amblflecasm.util.SafeString(json.locstatecode, "Unspecified")),
					string.Format("Profile URL  : {0}", amblflecasm.util.SafeString(json.profileurl)),
					string.Format("Date Created : {0}", dateCreatedFinal)
				});

				await this.ModifyOriginalResponseAsync(message => message.Content = box);
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get profile information");
			}
		}
	}
}
