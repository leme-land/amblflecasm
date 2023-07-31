using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Text;

namespace amblflecasm.data
{
	public class util
	{
		private static Random rng = new Random();
		private static string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		private static List<string> denials = new List<string>()
		{
			"No", "Nah", "Nuh uh", "Access is denied", "Make me", "Nop", "Nah, I don't really feel like it", "Why don't you ask me later?", "I will do no such thing"
		};

		public int FloorMod(object x, object y)
		{
			double a = 1, b = 1;

			double.TryParse(x.ToString(), out a);
			double.TryParse(y.ToString(), out b);

			return (int)(a - b * Math.Floor(a / b));
		}

		public string Plural(string message, int x, string not = "", string yes = "s") // Awesome variable naming
		{
			return x == 1 ? message + not : message + yes;
		}

		public string RandomString(int length = 10)
		{
			if (length < 1)
				return string.Empty;

			StringBuilder builder = new StringBuilder();

			for (int i = 1; i <= length; i++)
				builder.Append(characters[rng.Next(characters.Length)]);

			return builder.ToString();
		}

		public string RandomDenial()
		{
			return denials[rng.Next(denials.Count)];
		}

		public string SafeString(object? data, string fallback = "Unknown")
		{
			string? converted = Convert.ToString(data);
			if (converted == null || converted.Equals(string.Empty))
				return fallback;

			return converted;
		}

		public string CapitalString(string data)
		{
			char first = data[0];
			if (!char.IsUpper(first))
			{
				first = char.ToUpper(first);

				char[] letters = data.ToCharArray();
				letters[0] = first;

				return new string(letters);
			}

			return data;
		}

		public string Box(string title, string[] content)
		{
			int contentWidth = 0;
			for (int i = 0; i < content.Length; i++)
				contentWidth = Math.Max(contentWidth, content[i].Length);

			int boxWidth = Math.Max(contentWidth, title.Length);
			if (boxWidth < 20)
				boxWidth = 20;

			string box = string.Format("┌{0}┐\n", new string('─', boxWidth + 2)); // Top edge

			double titlePadding = boxWidth - title.Length;
			int leftPadding = (int)((titlePadding / 2) + (titlePadding % 2 == 0 ? 1 : 2));
			int rightPadding = (int)((titlePadding / 2) + 1);

			box += string.Format("│{0}{1}{2}│\n", new string(' ', leftPadding), title, new string(' ', rightPadding)); // Title row
			box += string.Format("├{0}┤\n", new string('─', boxWidth + 2)); // Title / Content spacer

			for (int i = 0; i < content.Length; i++)
			{
				if (content[i].Equals(string.Empty))
					box += string.Format("├{0}┤\n", new string('─', boxWidth + 2)); // Inserted spacer
				else
					box += string.Format("│ {0}{1} │\n", content[i], new string(' ', boxWidth - content[i].Length));
			}

			box += string.Format("└{0}┘", new string('─', boxWidth + 2)); // Bottom edge

			return "```\n" + box + "\n```";
		}

		public string FancyBox(string title, Dictionary<string, string> data, bool flipKeys = false)
		{
			int biggestName = 0;
			foreach (KeyValuePair<string, string> entry in data)
				biggestName = Math.Max(biggestName, (flipKeys ? entry.Value : entry.Key).Length);

			List<string> content = new List<string>();
			foreach (KeyValuePair<string, string> entry in data)
			{
				string key = flipKeys ? entry.Value : entry.Key;
				string value = flipKeys ? entry.Key : entry.Value;

				content.Add(string.Format("{0} {1}:  {2}", key, new string(' ', biggestName - key.Length), value));
			}

			return Box(title, content.ToArray());
		}

		public List<string> SplitBox(string title, Dictionary<string, string> data, int split = 10, bool flipKeys = false)
		{
			Dictionary<string, string> chunk = new Dictionary<string, string>();
			List<string> boxes = new List<string>();

			int length = data.Count();
			for (int i = 0; i < length; i++)
			{
				KeyValuePair<string, string> entry = data.ElementAt(i);
				chunk.Add(entry.Key, entry.Value);

				if ((i > 0 && i % split == 0) || i == length - 1)
				{
					boxes.Add(FancyBox(title, chunk, flipKeys));
					chunk.Clear();
				}
			}

			return boxes;
		}

		public DateTime GetTimestamp(int time = 0)
		{
			return DateTimeOffset.FromUnixTimeSeconds(time).DateTime;
		}

		public bool IsUser(SocketUser user, string name)
		{
			try
			{
				JArray? list = (JArray?)amblflecasm.config.GetObject("users", name);
				if (list == null)
					return false;

				List<ulong> users = list.ToObject<List<ulong>>();
				foreach (ulong id in users)
					if (user.Id == id)
						return true;

				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return false;
			}
		}

		public SocketChannel? FindChannelInGuild(ulong guildID, ulong channelID)
		{
			SocketGuild? guild = amblflecasm.socketClient.GetGuild(guildID);
			if (guild == null)
				return null;

			return guild.GetChannel(channelID);
		}

		public async Task SendMessageCopy(SocketMessage message, DiscordWebhookClient webhookClient)
		{
			string username = message.Author.Username;
			string avatarURL = message.Author.GetAvatarUrl();

			if (!message.Content.Equals(string.Empty))
				await webhookClient.SendMessageAsync(message.Content, false, message.Embeds, username, avatarURL, null, AllowedMentions.None);

			foreach (Discord.Attachment attachment in message.Attachments)
				await webhookClient.SendMessageAsync(attachment.Url, false, null, username, avatarURL);

			foreach (SocketSticker sticker in message.Stickers)
				await webhookClient.SendMessageAsync(sticker.GetStickerUrl(), false, null, username, avatarURL);
		}

		public async Task DenyInteraction(SocketInteractionContext context)
		{
			try
			{
				await context.Interaction.RespondAsync(RandomDenial());
			}
			catch (Exception) { }
		}
	}
}
