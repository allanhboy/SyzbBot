using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace WxBot
{
	public class WechatLoginXmlReader
	{
		private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

		public WechatLoginXmlReader(string xml)
		{
			if (string.IsNullOrEmpty(xml))
				return;

			var xdoc = XDocument.Parse(xml, LoadOptions.None);
			var root = xdoc.Element("error");

			if (root != null)
			{
				foreach (var element in root.Elements())
				{
					_dictionary.Add(element.Name.LocalName, element.Value);
				}
			}
		}

		public string this[string key]
		{
			get
			{
				_dictionary.TryGetValue(key, out var value);
				return value;
			}
		}

		public bool Success => !string.IsNullOrEmpty(PassTicket) && !string.IsNullOrEmpty(Skey) && !string.IsNullOrEmpty(Sid) && !string.IsNullOrEmpty(Uin);

		public dynamic GetBaseRequest()
		{
			return new { Uin, Sid, Skey, DeviceID };
		}

		public string CreateBaseRequestData()
		{
			var data = new { BaseRequest = new { Uin, Sid, Skey, DeviceID } };
			var val = JsonConvert.SerializeObject(data);
			return val;
		}

		public string CreateSycnRequestData(SyncKey syncKey)
		{
			var data = new { BaseRequest = new { Uin, Sid, Skey, DeviceID }, SyncKey = syncKey, rr = Common.ConvertDateTimeToInt(DateTime.Now.ToUniversalTime()) };
			var val = JsonConvert.SerializeObject(data);
			return val;
		}

		public string Uin => this["wxuin"];
		public string Sid => this["wxsid"];
		public string Skey => this["skey"];
		public string PassTicket => this["pass_ticket"];
		public string DeviceID { get; } = $"e{new Random().NextDouble().ToString("f16").Replace(".", string.Empty).Substring(1)}";
	}
}