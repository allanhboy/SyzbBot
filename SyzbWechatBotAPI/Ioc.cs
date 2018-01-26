using System.Data;
using System.Data.SqlClient;
using Autofac;
using Autofac.Core;
using Hangfire;
using SyzbWechatBotAPI.Jobs;

namespace SyzbWechatBotAPI
{
	public static class Ioc
	{
		public static IContainer Initialize(AppSettings appSettings)
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<SqlConnection>()
				.WithParameter(new ResolvedParameter(
					(pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "connectionString",
					(pi, ctx) => appSettings.SqlConnectionString))
				.As<IDbConnection>()
				.InstancePerBackgroundJob();

			builder.Register(c => appSettings);

			builder.RegisterType<LongRunningJob>();
			builder.RegisterType<MonitorJob>();
			builder.RegisterType<NewsJob>();

			return builder.Build();
		}
	}
}
