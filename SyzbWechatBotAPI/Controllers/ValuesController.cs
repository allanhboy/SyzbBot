using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SyzbWechatBotAPI.Jobs;

namespace SyzbWechatBotAPI.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		// GET api/values
		[HttpGet]
		public IEnumerable<string> Get()
		{
			//var tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
			BackgroundJob.Enqueue<NewsJob>(job => job.Dowload("http://www.my.gov.cn/jiangyou/288798823663271936/20170728/2012616.html", null));
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody]string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
