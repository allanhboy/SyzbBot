using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WxBot
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Task.WaitAll(new WechatBot().Run());
			//var obj = new Test { Uin = "1234" };
			//Console.WriteLine(JsonConvert.SerializeObject(obj));
			
			Console.Read();
		}
	}

	public class Test
	{
		public string Uin { get; set; }
	}
}
