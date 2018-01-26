﻿using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Hangfire.Console;
using Hangfire.Server;
using HtmlAgilityPack;
using NReadability;

namespace SyzbWechatBotAPI.Jobs
{
	public class NewsJob
	{
		private readonly IDbConnection _connection;

		public NewsJob(IDbConnection connection)
		{
			_connection = connection;
		}

		public async Task Dowload(string url, PerformContext context)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36");
				var response = await client.GetAsync(url);
				if (response.StatusCode != HttpStatusCode.OK)
					return;

				var stream = await response.Content.ReadAsStreamAsync();
				byte[] bytes = new byte[stream.Length];
				await stream.ReadAsync(bytes, 0, bytes.Length);

				var html = Encoding.UTF8.GetString(bytes);
				var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
				document.LoadHtml(html);

				HtmlNode contentTypeMeta = document.DocumentNode.SelectNodes("//meta").FirstOrDefault(selectNode => selectNode.Attributes["http-equiv"].Value == "Content-Type");

				if (contentTypeMeta != null)
				{
					var contentType = contentTypeMeta.Attributes["content"].Value;
					var match = Regex.Match(contentType, "charset=(?<encoding>[a-zA-Z0-9\\-]*)");
					if (match.Success)
					{
						var encodingName = match.Groups["encoding"].Value;
						html = Encoding.GetEncoding(encodingName).GetString(bytes);
					}

				}


				//var html = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(html))
					return;

				var transcoder = new NReadabilityTranscoder();
				var input = new TranscodingInput(html);
				try
				{
					var result = transcoder.Transcode(input);

					document.LoadHtml(result.ExtractedContent);
					var node = document.DocumentNode.SelectSingleNode("//div/div/div/div");
					var text = node.InnerText.Trim('\r', '\n', ' ', '\t');
					context.WriteLine("抽取内容为:");
					context.WriteLine(text);

					const string cmdText = @"UPDATE [dbo].[BaiduNews] SET [Html]=@Html,[Text]=@Text WHERE [Url]=@Url";

					await _connection.ExecuteAsync(cmdText, new { Html = html, Text = text, Url = url });
				}
				catch (Exception e)
				{
					context.WriteLine(e);
				}
			}
		}
	}
}