using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace amblflecasm.data
{
	public class config
	{
		private static dynamic configData;

		public Task<bool> LoadConfig()
		{
			Assembly self = Assembly.GetExecutingAssembly();
			if (self == null)
				throw new Exception("[!] Failed to get assembly");

			Stream? stream = self.GetManifestResourceStream("amblflecasm.data.config.json");
			if (stream == null)
				throw new Exception("[!] Failed to open 'config.json'");

			StreamReader reader = new StreamReader(stream);
			configData = JsonConvert.DeserializeObject(reader.ReadToEnd());

			reader.Close();
			reader.Dispose();
			stream.Dispose();

			return Task.FromResult(true);
		}

		private bool ObjectIsType(object? obj, Type type = null)
		{
			if (type == null) return obj == null;
			if (obj == null) return false;

			return obj.GetType().IsAssignableFrom(type);
		}

		public object? GetObject(params string[] keys)
		{
			if (keys.Length < 1)
				return null;

			object? obj = configData[keys[0]];
			int next = 1;

			while (ObjectIsType(obj, typeof(JArray)) && next < keys.Length)
			{
				foreach (JObject jObject in (JArray)obj)
					foreach (JProperty property in jObject.Properties())
						if (property.Name.Equals(keys[next]))
						{
							obj = property.Value;
							break;
						}

				next++;
			}

			return obj;
		}

		public string GetString(params string[] keys)
		{
			object? obj = GetObject(keys);
			if (obj == null)
				return string.Empty;

			return obj.ToString() ?? string.Empty;
		}
	}
}
