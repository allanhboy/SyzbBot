using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Hangfire.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using SyzbWechatBotAPI.Jobs;

namespace SyzbWechatBotAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;

			var appSettings = new AppSettings();
			Configuration.GetSection("AppSettings").Bind(appSettings);
			AppSettings = appSettings;
		}

		public IConfiguration Configuration { get; }
		public AppSettings AppSettings { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info
				{
					Title = "My API",
					Version = "v1"
				});
			});

			services.AddHangfire(x =>
			{
				x.UseRedisStorage(AppSettings.RedisHost,
					new RedisStorageOptions
					{
						FetchTimeout = TimeSpan.FromMilliseconds(500),
						Db = AppSettings.RedisDb
					});

				x.UseConsole();

				x.UseRecurringJob(typeof(LongRunningJob));

				x.UseAutofacActivator(Ioc.Initialize(AppSettings));
			});



		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterType<SqlConnection>()
				.WithParameter(new ResolvedParameter(
					(pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "connectionString",
					(pi, ctx) => AppSettings.SqlConnectionString))
				.As<IDbConnection>()
				.InstancePerLifetimeScope();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();

				app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
				{
					HotModuleReplacement = true,
					ReactHotModuleReplacement = true
				});
			}

			

			app.UseStaticFiles();

			app.UseSwagger();

			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});

			app.UseHangfireServer();
			app.UseHangfireDashboard("/jobs");

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");

				routes.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Home", action = "Index" });
			});
		}
	}
}
