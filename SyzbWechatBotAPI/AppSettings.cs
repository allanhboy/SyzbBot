using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyzbWechatBotAPI
{
	public class AppSettings
	{
		public string RedisHost { get; set; }
		public int RedisDb { get; set; }
		public string SqlConnectionString { get; set; }
	}
}
