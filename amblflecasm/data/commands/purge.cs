using Discord;
using Discord.Interactions;

namespace amblflecasm.data.commands
{
	public class purge : InteractionModuleBase<SocketInteractionContext>
	{
		public enum DeleteMode
		{
			Bulk,
			Loop
		}

		[SlashCommand("purge", "Take out the trash", false, RunMode.Async)]
		public async Task run(int amount = 100, DeleteMode deleteMode = DeleteMode.Bulk)
		{
			IGuildUser guildUser = (IGuildUser)this.Context.User;
			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ManageMessages)
			{
				await amblflecasm.util.DenyInteraction(this.Context);
				return;
			}

			await this.RespondAsync(string.Format("Attempting to delete {0} {1}", amount, amblflecasm.util.Plural("message", amount)));
			ulong responseID = this.GetOriginalResponseAsync().Result.Id;

			IEnumerable<IMessage> messages = await this.Context.Channel.GetMessagesAsync(amount).FlattenAsync();
			if (messages.Count() < 1)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "No messages found to purge");
				return;
			}

			try
			{
				if (deleteMode == DeleteMode.Loop) // Epic 2 week bypass, downside is it is ungodly slow
				{
					int deleted = 0;

					foreach (IMessage message in messages)
					{
						if (message.Id == responseID)
							continue;

						try
						{
							await message.DeleteAsync();
							deleted++;
						}
						catch (Exception) { }
					}

					amount = deleted;
				}
				else
				{
					messages = messages.Where(message => (DateTimeOffset.UtcNow - message.Timestamp).TotalDays <= 14 && message.Id != responseID); // Gay bulk delete gets owned
					amount = messages.Count();

					await ((ITextChannel)(this.Context.Channel)).DeleteMessagesAsync(messages);
				}

				await this.ModifyOriginalResponseAsync(message => message.Content = string.Format("Finished deleting {0} {1}", amount, amblflecasm.util.Plural("message", amount)));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to delete messages");
			}
		}
	}
}
