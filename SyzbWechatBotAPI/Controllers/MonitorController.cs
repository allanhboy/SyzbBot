using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyzbWechatBotAPI.Jobs;
using SyzbWechatBotAPI.Models;

namespace SyzbWechatBotAPI.Controllers
{
	[Route("api/Monitor")]
	public class MonitorController : Controller
	{
		private readonly IDbConnection _connection;

		public MonitorController(IDbConnection connection)
		{
			_connection = connection;
		}

		[HttpPost]
		public async Task<string> Post(MonitorRequestModel model)
		{
			var monitor = await _connection.QuerySingleOrDefaultAsync<Monitor>("SELECT * FROM [dbo].[Monitor] WHERE [Name]=@Name", new { model.Name });
			if (monitor == null)
			{
				await _connection.ExecuteAsync(
					@"INSERT INTO [dbo].[Monitor]([Type],[Name],[Remarks]) VALUES(@Type, @Name, @Remarks)", new { model.Type, model.Name, Remarks = model.NickName });

				BackgroundJob.Enqueue<MonitorJob>(job => job.Monitor(model.Name, null));

				return $"@{model.NickName}:监控设置成功.";
			}
			else
			{
				await _connection.ExecuteAsync("UPDATE [dbo].[Monitor] SET [Remarks]=[Remarks]+','+@NickName WHERE [Id]=@Id",
					new { monitor.Id, model.NickName });

				var news = await _connection.QueryAsync("SELECT TOP 5 * FROM [dbo].[BaiduNews] WHERE [Keyword]=@Keyword",
					new { Keyword = model.Name });

				var enumerable = news as dynamic[] ?? news.ToArray();
				if (enumerable.Any())
				{
					var index = 1;
					string content = enumerable.Aggregate("", (current, art) => current + $"{index++}.{art.Title}\r\n");
					return $"@{model.NickName}:{model.Name}最新舆情:\r\n{content}点击查看更多详情http://news.baidu.com/ns?word={model.Name}&tn=news&sr=0&cl=2&rn=50&ct=0&clk=sortbytime";
				}
				else
				{
					return $"@{model.NickName}:{model.Name}目前没有最新舆情!";
				}
			}
		}
	}
}