using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;

namespace amblflecasm.data.commands
{
	[Group("guild", "Guild commands")]
	public class guild : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("unload", "Unload slash commands from this guild", false, RunMode.Async)]
		public async Task unload()
		{
			IGuildUser guildUser = (IGuildUser)this.Context.User;
			if (!guildUser.GuildPermissions.Administrator)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			await this.RespondAsync("Attempting to unload commands");

			try
			{
				SocketGuild guild = (SocketGuild)this.Context.Guild;

				await guild.BulkOverwriteApplicationCommandAsync(new ApplicationCommandProperties[] { });

				await this.ModifyOriginalResponseAsync(message => message.Content = "Commands unloaded!");
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to unload guild commands");
			}
		}

		private static readonly int MAX_GUILDS_PER_PAGE = 10;

		[SlashCommand("list", "Lists guilds the bot is in", false, RunMode.Async)]
		public async Task list()
		{
			if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			await this.RespondAsync("Getting guilds");

			try
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				foreach (SocketGuild guild in this.Context.Client.Guilds)
					data.Add(guild.Id.ToString(), guild.Name);

				List<string> boxes = amblflecasm.util.SplitBox("Guild List", data, 10, true);

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
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get guilds");
			}
		}

		private static readonly int MAX_CHANNELS_PER_PAGE = 10;

		[SlashCommand("channels", "Lists the channels in a guild", false, RunMode.Async)]
		public async Task channels(string id)
		{
			if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			ulong parsedID;
			if (!ulong.TryParse(id.Trim(), out parsedID))
			{
				await this.RespondAsync("Invalid guild ID");
				return;
			}

			SocketGuild? guild = this.Context.Client.Guilds.First(guild => guild.Id == parsedID);
			if (guild == null)
			{
				await this.RespondAsync("I'm not in that guild");
				return;
			}

			await this.RespondAsync("Getting channels");

			try
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				foreach (SocketGuildChannel channel in guild.Channels)
				{
					if (channel.GetChannelType() == ChannelType.Category)
						continue;

					data.Add(channel.Id.ToString(), channel.Name);
				}

				List<string> boxes = amblflecasm.util.SplitBox("Channel List", data, 10, true);

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
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get channels");
			}
		}

		private static readonly string leaveFormatString = "Attempting to leave {0}";

		[SlashCommand("leave", "Leave a guild", false, RunMode.Async)]
		public async Task leave(string id)
		{
			if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			ulong parsedID;
			if (!ulong.TryParse(id.Trim(), out parsedID))
			{
				await this.RespondAsync("Invalid guild ID");
				return;
			}

			SocketGuild? guild = this.Context.Client.Guilds.First(guild => guild.Id == parsedID);
			if (guild == null)
			{
				await this.RespondAsync("I'm not in that guild");
				return;
			}

			await this.RespondAsync(string.Format(leaveFormatString, guild.Name));

			try
			{
				await guild.LeaveAsync();
				await this.ModifyOriginalResponseAsync(message => message.Content = "Peace out!");
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to leave (???)");
			}
		}

		[SlashCommand("members", "View guild members", false, RunMode.Async)]
		public async Task members(string id)
		{
			if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			ulong parsedID;
			if (!ulong.TryParse(id.Trim(), out parsedID))
			{
				await this.RespondAsync("Invalid guild ID");
				return;
			}

			SocketGuild? guild = this.Context.Client.Guilds.First(guild => guild.Id == parsedID);
			if (guild == null)
			{
				await this.RespondAsync("I'm not in that guild");
				return;
			}

			await this.RespondAsync("Getting members...");

			try
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				foreach (SocketUser user in guild.Users)
					data.Add(user.Username, user.Id.ToString());

				List<string> boxes = amblflecasm.util.SplitBox("User List", data, 10, false);

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
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get member list");
			}
		}

		private static string inviteFormat = "Here you go!\n{0}";

		[SlashCommand("invite", "Get an invite", false, RunMode.Async)]
		public async Task invite(string id)
		{
			if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			ulong parsedID;
			if (!ulong.TryParse(id.Trim(), out parsedID))
			{
				await this.RespondAsync("Invalid guild ID");
				return;
			}

			SocketGuild? guild = this.Context.Client.Guilds.First(guild => guild.Id == parsedID);
			if (guild == null)
			{
				await this.RespondAsync("I'm not in that guild");
				return;
			}

			await this.RespondAsync("Getting invite...");

			try
			{
				IReadOnlyCollection<Discord.Rest.RestInviteMetadata> invites = await guild.GetInvitesAsync();

				if (invites.Count < 1)
				{
					await this.ModifyOriginalResponseAsync(message => message.Content = "Server has no invites");
					return;
				}

				Discord.Rest.RestInviteMetadata? invite = invites.First((invite) =>
				{
					if (invite.ExpiresAt != null && DateTimeOffset.UtcNow >= invite.ExpiresAt)
						return false;

					if (invite.MaxUses > 0 && invite.Uses >= invite.MaxUses)
						return false;

					return true;
				});

				if (invite == null)
					throw new Exception();

				await this.ModifyOriginalResponseAsync(message => message.Content = string.Format(inviteFormat, invite.Url));
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get an invite");
			}
		}

		[Group("mirror", "Mirror")]
		public class guild_mirror : InteractionModuleBase<SocketInteractionContext>
		{
			private static ulong currentGuildID = 0; // ID of the guild where the original channel to be mirroed exists
			private static ulong currentChannelID = 0; // ID of the original channel to mirror
			private static ulong mirrorGuildID = 0; // ID of the guild where the mirror channel exists
			private static ulong mirrorChannelID = 0; // ID of the channel where the mirror will be output

			private void StopMirror()
			{
				currentGuildID = 0;
				currentChannelID = 0;
				mirrorGuildID = 0;
				mirrorChannelID = 0;
			}

			public async Task MessageReceivedHandler(SocketMessage message)
			{
				SocketUserMessage uMessage = (SocketUserMessage)message;
				SocketGuildChannel sChannel = (SocketGuildChannel)uMessage.Channel;
				if (sChannel.Guild.Id != currentGuildID || uMessage.Channel.Id != currentChannelID)
					return;

				SocketChannel? mirrorChannel = amblflecasm.util.FindChannelInGuild(mirrorGuildID, mirrorChannelID);
				if (mirrorChannel == null)
				{
					StopMirror();
					return;
				}

				IWebhook? webhook = null;
				IReadOnlyCollection<IWebhook?> webhooks = await (mirrorChannel as IIntegrationChannel).GetWebhooksAsync();
				if (webhooks.Count() < 1 || webhooks.ElementAt(0) == null)
					webhook = await (mirrorChannel as IIntegrationChannel).CreateWebhookAsync("mirror");
				else
					webhook = webhooks.ElementAt(0);

				if (webhook != null)
				{
					DiscordWebhookClient webhookClient = new DiscordWebhookClient(webhook);
					await amblflecasm.util.SendMessageCopy(message, webhookClient);
				}
			}

			private static bool handlerAdded = false;
			private static readonly string startedFormatString = "Started mirroring `{0}`";

			[SlashCommand("start", "Mirror a channel's messages", false, RunMode.Async)]
			public async Task start(string guildID, string channelID)
			{
				if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
				{
					await amblflecasm.util.DenyInteraction(this.Context);
					return;
				}

				if (currentGuildID != 0 || currentChannelID != 0 || mirrorGuildID != 0 || mirrorChannelID != 0)
				{
					await this.RespondAsync("There is already an active mirror");
					return;
				}

				ulong parsedGuildID;
				if (!ulong.TryParse(guildID.Trim(), out parsedGuildID))
				{
					await this.RespondAsync("Invalid guild ID");
					return;
				}

				ulong parsedChannelID;
				if (!ulong.TryParse(channelID.Trim(), out parsedChannelID))
				{
					await this.RespondAsync("Invalid channel ID");
					return;
				}

				SocketChannel? targetChannel = amblflecasm.util.FindChannelInGuild(parsedGuildID, parsedChannelID);
				if (targetChannel == null)
				{
					await this.RespondAsync("That channel does not exist");
					return;
				}

				currentGuildID = parsedGuildID;
				currentChannelID = parsedChannelID;
				mirrorGuildID = this.Context.Guild.Id;
				mirrorChannelID = this.Context.Channel.Id;

				if (!handlerAdded)
				{
					this.Context.Client.MessageReceived += MessageReceivedHandler;
					handlerAdded = true;
				}

				await this.RespondAsync(string.Format(startedFormatString, (targetChannel as SocketGuildChannel).Name));
			}

			[SlashCommand("stop", "Stop mirror", false, RunMode.Async)]
			public async Task stop()
			{
				if (!amblflecasm.util.IsUser(this.Context.User, "leme"))
				{
					await amblflecasm.util.DenyInteraction(this.Context);
					return;
				}

				if (currentGuildID == 0 || currentChannelID == 0 || mirrorGuildID == 0 || mirrorChannelID == 0)
				{
					await this.RespondAsync("Nothing is being mirrored");
					return;
				}

				StopMirror();

				await this.RespondAsync("Mirror stopped");
			}
		}
	}
}
