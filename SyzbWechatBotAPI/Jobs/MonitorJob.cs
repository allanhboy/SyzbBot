using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using HtmlAgilityPack;

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
                //var author = element.SelectSingleNode(".//div/p[@class='c-author']/text()").InnerText;
                var summary_element = element.SelectSingleNode(".//div[@class='c-summary c-row ']");
                if (summary_element == null)
                {
                    summary_element = element.SelectSingleNode(".//div[@class='c-summary c-row c-gap-top-small']/div[@class='c-span18 c-span-last']");
                }

                var author_element = summary_element.SelectSingleNode(".//p[@class='c-author']");
                var info_element = summary_element.SelectSingleNode(".//span[@class='c-info']");

                summary_element.RemoveChild(author_element);
                summary_element.RemoveChild(info_element);

                var author = author_element.InnerText;
                var summary = summary_element.InnerText.Replace("<em>", "").Replace("</em>", "").Replace("&nbsp;", "");

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
                    @"INSERT INTO [dbo].[BaiduNews] (Keyword,Title,Url,Summary,Time) VALUES(@Keyword,@Title,@Url,@Summary,@Time)",
                    new { Keyword = name, Title = title, Url = url, Summary = summary, Time = time });



                BackgroundJob.ContinueWith<NewsJob>(context.BackgroundJob.Id, job => job.Dowload(url, null));
            }
        }

    }
}
