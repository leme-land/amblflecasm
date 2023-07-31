using amblflecasm.data;
using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace amblflecasm
{
	public class amblflecasm
	{
		private IServiceProvider? serviceProvider;
		private DiscordSocketConfig? socketConfig;
		public static DiscordSocketClient? socketClient;
		public static InteractionService? interactionService;

		public static config? config;
		public static util? util;

		private Task LogHandler(LogMessage message)
		{
			Console.WriteLine(message.ToString());
			return Task.CompletedTask;
		}

		public void Log(string message, string source = "Unknown", LogSeverity severity = LogSeverity.Info)
		{
			LogHandler(new LogMessage(severity, source, message));
		}

		public async Task ReadyHandler()
		{
			await socketClient.SetActivityAsync(new Game("niggers choke and die", ActivityType.Watching));

			Log("Registering commands", "ReadyHandler");

			try
			{
#if DEBUG
				await interactionService.RegisterCommandsToGuildAsync(642793766188744715, true);
#else
				await interactionService.RegisterCommandsGloballyAsync(true);
#endif

				Log("Successfully registered commands", "ReadyHandler");
			}
			catch (Exception)
			{
				Log("[!] Failed to register commands", "ReadyHandler", LogSeverity.Error);
			}
		}

		public async Task InteractionHandler(SocketInteraction interaction)
		{
			if (interaction.Type == InteractionType.MessageComponent) // Buttons
				return;

			try
			{
				SocketInteractionContext context = new SocketInteractionContext(socketClient, interaction);
				IResult result = await interactionService.ExecuteCommandAsync(context, serviceProvider);

				if (!result.IsSuccess)
				{
					try
					{
						await interaction.RespondAsync("Something broke. Dumbfuck leme");
					}
					catch (Exception) { }

					Log(result.ErrorReason, "InteractionHandler", LogSeverity.Error);
				}
			}
			catch (Exception ex)
			{
				try
				{
					await interaction.RespondAsync("Something broke. Dumbfuck leme");
				}
				catch (Exception) { }

				Console.WriteLine(ex.ToString());
				Log(ex.Message, "InteractionHandler", LogSeverity.Error);
			}
		}

		public async Task MessageHandler(SocketMessage message)
		{
			if (message.Author.IsBot)
				return;

			string? webhookURL = null;
			ulong webhookChannelID = 0;

			JArray? webhookData = (JArray?)config.GetObject("webhooks", "tjelc");
			if (webhookData != null)
			{
				webhookURL = webhookData[0].ToString();
				webhookChannelID = ulong.Parse(webhookData[1].ToString());
			}
			if (webhookURL == null || message.Channel.Id != webhookChannelID)
				return;

			await util.SendMessageCopy(message, new DiscordWebhookClient(webhookURL));
			await message.DeleteAsync();
		}

		public static Task Main() => new amblflecasm().MainAsync();
		private async Task MainAsync()
		{
			util = new util();
			if (util == null)
				throw new Exception("[!] Failed to create util");

			config = new config();
			if (config == null)
				throw new Exception("[!] Failed to create config");

			if (!(await config.LoadConfig()))
				throw new Exception("[!] Failed to load config");

			socketConfig = new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
				AlwaysDownloadUsers = true
			};

			socketClient = new DiscordSocketClient(socketConfig);

			socketClient.Log += LogHandler;
			socketClient.Ready += ReadyHandler;
			socketClient.InteractionCreated += InteractionHandler;
			socketClient.MessageReceived += MessageHandler;

			interactionService = new InteractionService(socketClient);
			serviceProvider = new ServiceCollection()
				.AddSingleton(socketClient)
				.AddSingleton(interactionService)
				.BuildServiceProvider();

			await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

			await socketClient.LoginAsync(TokenType.Bot, config.GetString("tokens", "bot"));
			await socketClient.StartAsync();

			await Task.Delay(Timeout.Infinite);
		}
	}
}