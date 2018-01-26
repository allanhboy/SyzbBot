using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using Hangfire;
using Hangfire.Console;
using Hangfire.Redis;
using Hangfire.Server;
using HtmlAgilityPack;
using NReadability;

namespace SyzbWechatBotAPI.Jobs
{
	public class MonitorJob
	{
		private readonly IDbConnection _connection;
		public MonitorJob(IDbConnection connection)
		{
			_connection = connection;
		}

		public async Task Monitor(string name, PerformContext context)
		{
			var web = new HtmlWeb()
			{
				UserAgent =
						"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36"
			};
			context.WriteLine($"开始获取数据:{name}");
			var doc = web.Load($"http://news.baidu.com/ns?word={name}&tn=news&sr=0&cl=2&rn=50&ct=0&clk=sortbytime");


			var elements = doc.DocumentNode.SelectNodes("//div[@class='result']");

			foreach (var element in elements)
			{
				var title = element.SelectSingleNode("h3[@class='c-title']/a/text()").InnerText;
				var url = element.SelectSingleNode("h3[@class='c-title']/a").Attributes["href"].Value;
				var author = element.SelectSingleNode(".//div/p[@class='c-author']/text()").InnerText;
				var time = string.Empty;
				try
				{
					time = author.Substring(author.IndexOf("&nbsp;&nbsp;", StringComparison.Ordinal) + 12);
				}
				catch (Exception e)
				{
					context.WriteLine($"{e}");
				}
				context.WriteLine($"{title} -- {time}");
				if (await _connection.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM [dbo].[BaiduNews] WHERE [Url]=@Url",
						new { Url = url }) != 0) continue;

				await _connection.ExecuteAsync(
					@"INSERT INTO [dbo].[BaiduNews] (Keyword,Title,Url,Time) VALUES(@Keyword,@Title,@Url,@Time)",
					new { Keyword = name, Title = title, Url = url, Time = time });



				BackgroundJob.ContinueWith<NewsJob>(context.BackgroundJob.Id, job => job.Dowload(url, null));
			}
		}

	}
}
