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
    [Route("api/News")]
    public class NewsController : Controller
    {
        private readonly IDbConnection _connection;

        public NewsController(IDbConnection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        [Route("Tag/{tag}")]
        public async Task<IEnumerable<BaiduNews>> GetTag(string tag)
        {
            var list = await _connection.QueryAsync<BaiduNews>("SELECT * FROM [dbo].[BaiduNews] WHERE [Keyword]=@Keyword", new { Keyword = tag });
            return list;
        }

        [HttpGet]
        public async Task<BaiduNews> Get(long id)
        {
            var news = await _connection.QuerySingleOrDefaultAsync<BaiduNews>("SELECT * FROM [dbo].[BaiduNews] WHERE [Id]=@Id", new { Id = id });
            return news;
        }
    }
}