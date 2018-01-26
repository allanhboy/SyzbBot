using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using HtmlAgilityPack;
using NReadability;

namespace SyzbWechatBotAPI.Jobs
{
	//[Properties(Owner = "Syzb", Developer = "bruke", Date = "2018-01-15", Subject = "百度搜索结果", Email = "10245353@qq.com")]
	[TaskName("BaiduNewsSpider")]
	public class BaiduNewsSpider : EntitySpider
	{
		public BaiduNewsSpider() : base("BaiduNewsSpider", new Site
		{
			UserAgent =
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36"
		})
		{

		}

		protected override void MyInit(params string[] arguments)
		{
			var word = arguments[0];
			AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&sr=0&cl=2&rn=50&ct=0&clk=sortbytime", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			AddPageProcessor(new BaiduProcessor());
			AddPipeline(new BaiduNewsPipeline());
		}
	}

	class BaiduProcessor : BasePageProcessor
	{
		protected override void Handle(Page page)
		{
			var elements = page.Selectable.SelectList(Selectors.XPath("//div[@class='result']")).Nodes();
			var results = new List<BaiduNews>();
			var keyword = page.Request.Extras.Aggregate("", (current, kv) => string.IsNullOrEmpty(current) ? kv.Value : $"{current},{kv.Value}");
			foreach (var element in elements)
			{
				var title = element.Select(Selectors.XPath("h3[@class='c-title']/a")).GetValue().Replace("<em>", "").Replace("</em>", "");
				var url = element.Select(Selectors.XPath("h3[@class='c-title']/a/@href")).GetValue();
				var author = element.Select(Selectors.XPath(".//div/p[@class='c-author']/text()")).GetValue();
				var time = string.Empty;
				try
				{
					time = author.Substring(author.IndexOf("&nbsp;&nbsp;", StringComparison.Ordinal) + 12);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}

				var news = new BaiduNews
				{
					Keyword = keyword,
					Title = title,
					Time = time,
					Url = url
				};
				page.AddTargetRequest(url, increaseDeep: false);

				results.Add(news);
			}
			page.AddResultItem("News", results);

			if (!results.Any())
			{
				//bool success;
				var transcoder = new NReadabilityTranscoder();
				var input = new TranscodingInput(page.Content)
				{
					//DomSerializationParams = new DomSerializationParams()
					//{
					//	DontIncludeDocTypeMetaElement = true,
					//	DontIncludeContentTypeMetaElement = true,
					//	DontIncludeGeneratorMetaElement = true,
					//	DontIncludeMobileSpecificMetaElements = true,
					//	PrettyPrint = true
					//}
				};
				var text = "";
				try
				{
					var result = transcoder.Transcode(input);
					var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
					document.LoadHtml(result.ExtractedContent);
					var node = document.DocumentNode.SelectSingleNode("//div/div/div/div");
					text = node.InnerText.Trim('\r', '\n', ' ');
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					//throw;
				}
				
				page.AddResultItem("UpdateNews", new UpdateNews
				{
					Html = page.Content,
					Text = text,
					Url = page.Url
				});
			}
		}
	}

}


class BaiduNews
{
	public string Title { get; set; }
	public string Url { get; set; }
	public string Keyword { get; set; }
	public string Time { get; set; }
}

class UpdateNews
{
	public string Html { get; set; }
	public string Url { get; set; }
	public string Text { get; set; }
}


class BaiduNewsPipeline : BasePipeline
{
	public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
	{
		foreach (var resultItem in resultItems)
		{
			using (var conn =
				new SqlConnection("server=139.196.144.98;database=baidu;uid=sa;pwd=1qaz@WSX3edc;MultipleActiveResultSets=true"))
			{
				conn.Open();
				if (resultItem.GetResultItem("News") != null)
				{
					const string cmdText = @"INSERT INTO [dbo].[BaiduNews] (Keyword,Title,Url,Time) VALUES(@Keyword,@Title,@Url,@Time)";
					const string selectText = @"SELECT COUNT(1) FROM [dbo].[BaiduNews] WHERE [Url]=@Url";
					foreach (BaiduNews news in resultItem.GetResultItem("News"))
					{
						var c = conn.ExecuteScalar<int>(selectText, news);
						if (c == 0)
							conn.Execute(cmdText, news);
					}
				}

				if (resultItem.GetResultItem("UpdateNews") != null)
				{
					const string cmdText = @"UPDATE [dbo].[BaiduNews] SET [Html]=@Html,[Text]=@Text WHERE [Url]=@Url";

					conn.Execute(cmdText, (UpdateNews)resultItem.GetResultItem("UpdateNews"));
				}
			}
		}
	}
}
