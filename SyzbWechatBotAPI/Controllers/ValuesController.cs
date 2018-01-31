using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using Microsoft.AspNetCore.Mvc;
using SyzbWechatBotAPI.Jobs;
using SyzbWechatBotAPI.Models;

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
            BackgroundJob.Enqueue<MonitorJob>(job => job.Monitor("日昌升", null));

            //var text = "日昌升集团有限公司";
            //var extractor = new TfidfExtractor();
            //var keywords = extractor.ExtractTags(text);

            yield return "ok";

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
