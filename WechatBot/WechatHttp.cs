using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace WxBot
{
	internal class WechatHttp
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private static readonly HttpMessageHandler Handler = new HttpClientHandler()
		{
			CookieContainer = new CookieContainer()
		};

		private const string UserAgent =
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36";

		public static async Task<string> Get(string url)
		{
			var client = new HttpClient(Handler, false);
			using (client)
			{

				client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
				var result = await client.GetStringAsync(url);
				if (Logger.IsTraceEnabled)
				{
					Logger.Trace(url);
					Logger.Trace(result);
				}
				return result;
			}
		}

		public static async Task<string> Post(string url, string data, string contentType = "application/x-www-form-urlencoded")
		{
			var client = new HttpClient(Handler, false);
			using (client)
			{

				client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
				var response = await client.PostAsync(url, new StringContent(data, Encoding.UTF8, contentType));
				var result = await response.Content.ReadAsStringAsync();
				if (Logger.IsTraceEnabled)
				{
					Logger.Trace(url);
					Logger.Trace(data);
					Logger.Trace(result);
				}
				return result;
			}
		}

		public static async Task<string> PostAsJson(string url, string data)
		{
			return await Post(url, data, "application/json");
		}
	}
}