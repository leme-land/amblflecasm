using Discord.Interactions;
using Discord.WebSocket;
using System.Drawing;
using System.Drawing.Imaging;

namespace amblflecasm.data.commands
{
	public class ascii : InteractionModuleBase<SocketInteractionContext>
	{
		private static readonly string characters = "(#$%@*+;:,.";
		private static readonly int IMAGE_WIDTH = 40;
		private static readonly int IMAGE_HEIGHT = 16;

		[SlashCommand("ascii", "ASCII someone", false, RunMode.Async)]
		public async Task run(SocketUser target)
		{
			await this.RespondAsync("Working");

			System.Drawing.Image? avatarImage = null;

			try
			{
				using (HttpClient httpClient = new HttpClient())
				{
					byte[] imageData = await httpClient.GetByteArrayAsync(target.GetAvatarUrl());
					avatarImage = new Bitmap(System.Drawing.Image.FromStream(new MemoryStream(imageData)), IMAGE_WIDTH, IMAGE_HEIGHT);
				}
			}
			catch (Exception)
			{
				avatarImage = null;
			}

			if (avatarImage == null)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to get avatar image");
				return;
			}

			Bitmap? newImage = null;

			try // Grayscale
			{
				newImage = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT);
				using (Graphics g = Graphics.FromImage(newImage))
				{
					ColorMatrix colorMatrix = new ColorMatrix(
						new float[][]
						{
							new float[] {.3f, .3f, .3f, 0, 0},
							new float[] {.59f, .59f, .59f, 0, 0},
							new float[] {.11f, .11f, .11f, 0, 0},
							new float[] {0, 0, 0, 1, 0},
							new float[] {0, 0, 0, 0, 1}
						});

					using (ImageAttributes attributes = new ImageAttributes())
					{
						attributes.SetColorMatrix(colorMatrix);
						g.DrawImage(avatarImage, new Rectangle(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT), 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT, GraphicsUnit.Pixel, attributes);
					}
				}
			}
			catch (Exception)
			{
				newImage = null;
			}

			if (newImage == null)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to convert image");
				return;
			}

			try // Parse
			{
				string result = string.Empty;

				for (int y = 0; y < IMAGE_HEIGHT; y++)
				{
					for (int x = 0; x < IMAGE_WIDTH; x++)
					{
						Color pixel = newImage.GetPixel(x, y);

						int brightness = pixel.R + pixel.G + pixel.B;
						int index = amblflecasm.util.FloorMod(brightness, characters.Length);

						result += characters[index];
					}

					result += "\n";
				}

				await this.ModifyOriginalResponseAsync(message => message.Content = string.Format("```\n{0}\n```", result));
			}
			catch (Exception)
			{
				await this.ModifyOriginalResponseAsync(message => message.Content = "Failed to generate ascii");
			}
		}
	}
}
