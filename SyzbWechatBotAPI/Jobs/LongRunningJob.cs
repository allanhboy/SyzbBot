using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;

namespace SyzbWechatBotAPI.Jobs
{
	public class LongRunningJob
	{
		private readonly IDbConnection _connection;

		public LongRunningJob(IDbConnection connection)
		{
			_connection = connection;
		}

		[RecurringJob("0 8-22 * * *", TimeZone = "China Standard Time")]
		//[Queue("longjobs")]
		public async Task Run(PerformContext context)
		{
			context.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} LongRunningJob Running ...");
			const string cmdText = @"SELECT * FROM [dbo].[Monitor]";
			var monitors = await _connection.QueryAsync(cmdText);

			context.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} Monitors:{monitors.Count()}");

			foreach (var monitor in monitors)
			{
				//await new BaiduNewsSpider().RunAsync(monitor.Name);
			}

		}
	}
}
