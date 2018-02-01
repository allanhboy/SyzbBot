using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using Hangfire;
using JiebaNet.Analyser;
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
                var name = model.Name;
                if (model.Type == MonitorType.公司)
                {
                    var extractor = new TfidfExtractor();
                    var keywords = extractor.ExtractTags(model.Name).ToArray();
                    name = keywords[0];
                }

                BackgroundJob.Enqueue<MonitorJob>(job => job.Monitor(name, null));

                return $"@{model.NickName}:监控设置成功.";
            }
            else
            {
                await _connection.ExecuteAsync("UPDATE [dbo].[Monitor] SET [Remarks]=[Remarks]+','+@NickName WHERE [Id]=@Id",
                    new { monitor.Id, model.NickName });
                var name = model.Name;
                if (model.Type == MonitorType.公司)
                {
                    var extractor = new TfidfExtractor();
                    var keywords = extractor.ExtractTags(model.Name).ToArray();
                    name = keywords[0];
                }
                var news = await _connection.QueryAsync<BaiduNews>("SELECT TOP 5 * FROM [dbo].[BaiduNews] WHERE [Keyword]=@Keyword ORDER BY [CreateDate] DESC",
                    new { Keyword = name });

                var enumerable = news as BaiduNews[] ?? news.ToArray();
                if (enumerable.Any())
                {
                    //await _connection.ExecuteAsync("UPDATE [dbo].[BaiduNews] SET [IsPushed]=1 WHERE Id=@Id",
                    //    enumerable.Where(p => p.IsPushed == false).Select(p => new { p.Id }));
                    var index = 1;
                    string content = enumerable.Aggregate("", (current, art) => current + $"{index++}.{art.Title}\r\n");
                    return $"@{model.NickName}:{model.Name}最新舆情:\r\n{content}点击查看更多详情http://localhost:65064/{HttpUtility.UrlEncode(model.Name)}";
                }
                else
                {
                    return $"@{model.NickName}:{model.Name}目前没有最新舆情!";
                }
            }
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var monitor = await _connection.QueryFirstOrDefaultAsync<Monitor>("SELECT * FROM [dbo].[Monitor] WHERE [NewsCount]>0");
            if (monitor == null)
                return "";

            var news = await _connection.QueryAsync<BaiduNews>(
                $"SELECT TOP {monitor.NewsCount} * FROM [dbo].[BaiduNews] WHERE [Keyword]=@Keyword ORDER BY [CreateDate] DESC",
                new { Keyword = monitor.Tag });

            var enumerable = news as BaiduNews[] ?? news.ToArray();
            if (enumerable.Any())
            {
                await _connection.ExecuteAsync("UPDATE [dbo].[BaiduNews] SET [IsPushed]=1 WHERE Id=@Id",
                    enumerable.Where(p => p.IsPushed == false).Select(p => new { p.Id }));
                var index = 1;
                string content = enumerable.Aggregate("", (current, art) => current + $"{index++}.{art.Title}\r\n");
                return $"{monitor.Remarks}:{monitor.Name}最新舆情:\r\n{content}点击查看更多详情http://localhost:65064/{HttpUtility.UrlEncode(monitor.Tag)}";
            }
            else
            {
                return "";
            }
        }
    }
}