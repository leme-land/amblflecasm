using Discord.Interactions;
using Newtonsoft.Json;

namespace amblflecasm.data.commands
{
	public class ipinfo : InteractionModuleBase<SocketInteractionContext>
	{
		private static string urlFormatString = "https://proxycheck.io/v2/{0}?vpn=1&asn=1"; // Not actually IP info >:)

		[SlashCommand("ipinfo", "IP info lookup", false, RunMode.Async)]
		public async Task run(string ipAddress)
		{
			await this.RespondAsync("Getting IP data");

			ipAddress = ipAddress.Trim();

			try
			{
				dynamic? json;

				using (HttpClient httpClient = new HttpClient())
				{
					string data = await httpClient.GetStringAsync(string.Format(urlFormatString, ipAddress));

					json = JsonConvert.DeserializeObject(data);
					if (json == null)
						throw new Exception();

					if (!json.status.ToString().ToLower().Equals("ok"))
						throw new Exception();

					if (json[ipAddress] == null)
						throw new Exception();
				}

				string box = amblflecasm.util.Box(ipAddress, new string[]
					{
						string.Format("Country      :            {0}", amblflecasm.util.SafeString(json[ipAddress].country)),
						string.Format("Region       :            {0}", amblflecasm.util.SafeString(json[ipAddress].region)),
						string.Format("City         :            {0}", amblflecasm.util.SafeString(json[ipAddress].city)),
						string.Format("Provider     :            {0}", amblflecasm.util.SafeString(json[ipAddress].provider)),
						string.Format("Organization :            {0}", amblflecasm.util.SafeString(json[ipAddress].organisation)),
						string.Empty,
						string.Format("Proxy/VPN    :            {0}", amblflecasm.util.CapitalString(amblflecasm.util.SafeString(json[ipAddress].proxy))),
						string.Format("Operator     :            {0}", amblflecasm.util.SafeString(json[ipAddress]["operator"]?.name))
					});

				await this.ModifyOriginalResponseAsync(message => message.Content = box);
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get IP data");
			}
		}
	}
}
