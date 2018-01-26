using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyzbWechatBotAPI.Models
{
	public class MonitorRequestModel
	{
		public MonitorType Type { get; set; }
		public string Name { get; set; }
		public string NickName { get; set; }
	}

	public enum MonitorType
	{
		公司 = 1,
		行业 = 2
	}
}
