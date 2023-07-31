using Discord.Interactions;

namespace amblflecasm.data.commands
{
	public class help : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("help", "Help I'm retarded", false, RunMode.Async)]
		public async Task run()
		{
			await this.RespondAsync("Getting list");

			Dictionary<string, string> data = new Dictionary<string, string>();

			foreach (SlashCommandInfo commandInfo in amblflecasm.interactionService.SlashCommands)
			{
				ModuleInfo moduleInfo = commandInfo.Module;
				if (moduleInfo != null)
				{
					string name = string.Empty;

					if (!moduleInfo.Name.Equals(commandInfo.Name))
						name = moduleInfo.Name + " " + commandInfo.Name;
					else
						name = commandInfo.Name;

					name = name.Replace('_', ' '); // :^)

					data.Add(name, commandInfo.Description);
				}
			}

			List<string> boxes = amblflecasm.util.SplitBox("Help", data, 15);

			if (boxes.Count == 1)
				await this.ModifyOriginalResponseAsync(message => message.Content = boxes[0]);
			else
			{
				await this.DeleteOriginalResponseAsync();

				foreach (string box in boxes)
					await this.Context.Channel.SendMessageAsync(box);
			}
		}
	}
}
