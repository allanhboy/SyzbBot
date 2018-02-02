using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace WxBot
{
	internal class WechatBot
	{
		/// <summary>
		/// 最大重试次数
		/// </summary>
		private const int MaxRetryTimes = 10;
		private const string Unkonwn = "unkonwn";
		/// <summary>
		/// 已扫描 已登录
		/// </summary>
		private const string Success = "200";
		/// <summary>
		/// 已扫描 未登录
		/// </summary>
		private const string Scaned = "201";
		/// <summary>
		/// 超时
		/// </summary>
		private const string Timeout = "408";
		/// <summary>
		/// 重试等待(秒)
		/// </summary>
		private const int TryLaterSecs = 1;

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private string _uuid;
		/// <summary>
		/// 登录成功跳转
		/// </summary>
		private string _redirectUri;
		/// <summary>
		/// 基础Uri
		/// </summary>
		private string _baseUri;

		private string _baseHost;
		/// <summary>
		/// 同步地址
		/// </summary>
		private string _syncHost;

		private WechatLoginXmlReader _loginXml;
		private SyncKey _syncKey;
		private User _me;
		private IEnumerable<User> _contactList = new List<User>();

		public async Task Run()
		{

			if (!await GetUUID())
			{
				Logger.Warn("登录失败:uuid获取失败");
				return;
			}

			if (!GenerateQRCode())
			{
				Logger.Warn("获取登录二维码失败");
				return;
			}

			Logger.Info("请使用微信扫一扫以登录");
			var loginCode = await WaitLogin();
			if (loginCode != Success)
			{
				Logger.Error("微信登录失败.错误误:{0}", loginCode);
				return;
			}

			if (!await Login())
			{
				Logger.Error("微信登录失败");
				return;
			}
			Logger.Info("微信登录成功.");

			if (!await Init())
			{
				Logger.Error("微信初始化失败.");
				return;
			}
			Logger.Info("微信初始化成功.");

			await StatusNotify();

			await GetContact();

			await ProcessMessage();

			Logger.Info("成功");
		}

		/// <summary>
		/// 获取uuid
		/// </summary>
		/// <returns></returns>
		private async Task<bool> GetUUID()
		{
			var url = $"https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb&fun=new&lang=zh_CN&_={Common.ConvertDateTimeToInt(DateTime.Now)}";

			var returnValue = await WechatHttp.Get(url);

			var match = Regex.Match(returnValue, "window.QRLogin.code = (?<code>\\d+); window.QRLogin.uuid = \"(?<uuid>\\S+?)\"");

			if (match.Success)
			{
				var code = match.Groups["code"].Value;
				_uuid = match.Groups["uuid"].Value;

				return code == "200";
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 获取登录二维码
		/// </summary>
		/// <returns></returns>
		private bool GenerateQRCode()
		{
			var url = $"https://login.weixin.qq.com/l/{_uuid}";
			Logger.Trace("QRCode:{0}", url);
			var qrcode = Common.GenerateQRCode(url);
			if (qrcode != null)
			{

				var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "qrcode", $"{_uuid}.png");
				qrcode.Save(filename, ImageFormat.Png);

				ProcessStartInfo info = new ProcessStartInfo
				{
					FileName = "mspaint.exe",
					Arguments = filename
				};
				Process.Start(info);

				return true;
			}

			return false;
		}
		/// <summary>
		/// 等待扫描检测
		/// </summary>
		/// <returns></returns>
		private async Task<string> WaitLogin()
		{
			//     http comet:
			//tip=1, 等待用户扫描二维码,
			//       201: scaned
			//       408: timeout
			//tip=0, 等待用户确认登录,
			//       200: confirmed

			var tip = "1";
			var retryTime = MaxRetryTimes;

			string code = null;

			while (retryTime > 0)
			{
				var url =
					$"https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?tip={tip}&uuid={_uuid}&_={Common.ConvertDateTimeToInt(DateTime.Now)}";

				var loginResult = await WechatHttp.Get(url);

				var match = Regex.Match(loginResult, "window.code=(?<code>\\d+)");
				if (match.Success)
				{
					code = match.Groups["code"].Value;
				}

				switch (code)
				{
					case Scaned:
						Logger.Info("请在手机上确认登录");
						tip = "0";
						break;
					case Success:
						match = Regex.Match(loginResult, "window.redirect_uri=\"(?<redirect_uri>\\S+?)\"");
						if (match.Success)
						{
							var redirectUri = $"{match.Groups["redirect_uri"].Value}&fun=new";
							_redirectUri = redirectUri;
							_baseUri = redirectUri.Substring(0, redirectUri.LastIndexOf('/'));
							var tempHost = _baseUri.Substring(8);
							_baseHost = tempHost.Substring(0, tempHost.IndexOf('/'));
							return code;
						}
						break;
					case Timeout:
						Logger.Warn("微信登录异常:{0}.{1}秒后重试", code, TryLaterSecs);
						tip = "1";
						retryTime -= 1;
						Thread.Sleep(TryLaterSecs * 1000);
						break;
					default:
						return null;
				}
				Thread.Sleep(800);
			}

			return code;
		}

		/// <summary>
		/// 获取skey sid uid pass_ticket
		/// </summary>
		/// <returns></returns>
		private async Task<bool> Login()
		{
			var result = await WechatHttp.Get(_redirectUri);

			_loginXml = new WechatLoginXmlReader(result);

			return _loginXml.Success;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		/// <returns></returns>
		private async Task<bool> Init()
		{
			var url = $"{_baseUri}/webwxinit?r={Common.ConvertDateTimeToInt(DateTime.Now)}&lang=en_US&pass_ticket={_loginXml.PassTicket}";
			var data = _loginXml.CreateBaseRequestData();

			var returnValue = await WechatHttp.Post(url, data);
			var result = JsonConvert.DeserializeObject<InitResponse>(returnValue);

			_syncKey = result.SyncKey;
			_me = result.User;
			if (result.Count > 0)
			{
				_contactList = result.ContactList;
			}

			return result?.BaseResponse?.Ret == 0;
		}

		private async Task<bool> StatusNotify()
		{
			var url = $"{_baseUri}/webwxstatusnotify?lang=zh_CN&pass_ticket={_loginXml.PassTicket}";

			var obj = new { BaseRequest = _loginXml.GetBaseRequest(), Code = 3, FromUserName = _me.UserName, ToUserName = _me.UserName, ClientMsgId = Common.ConvertDateTimeToInt(DateTime.Now) };
			var data = JsonConvert.SerializeObject(obj);

			var result = await WechatHttp.Post(url, data);
			var response = JsonConvert.DeserializeObject<StatusNotifyResponse>(result);

			return response.BaseResponse.Ret == 0;
		}

		private async Task GetContact()
		{
			var url = $"{_baseUri}/webwxgetcontact?pass_ticket={_loginXml.PassTicket}&skey={_loginXml.Skey}&r={Common.ConvertDateTimeToInt(DateTime.Now)}";

			var result = await WechatHttp.Post(url, "{}");

			var response = JsonConvert.DeserializeObject<GetContactResponse>(result);
			if (response.MemberCount > 0)
			{
				_contactList = response.MemberList;
			}

		}

		private async Task ProcessMessage()
		{
			await TestSyncCheck();
			var watch = Stopwatch.StartNew();
			while (true)
			{
				watch.Reset();
				var returnArray = await SyncCheck();
				var retcode = returnArray[0];
				var selector = returnArray[1];

				if (retcode == "1100")  //从微信客户端上登出
					break;
				else if (retcode == "1101") // 从其它设备上登了网页微信
					break;
				else if (retcode == "0")
				{
					switch (selector)
					{
						case "2": // 有新消息
							var rsp = await Sync();
							if (rsp != null)
							{
								await HandleMessage(rsp);
							}
							break;
						case "3": // 未知
							break;
						case "4": //通讯录更新
							break;
						case "6": //可能是红包
							break;
						case "7": //在手机上操作了微信
							break;
						case "0": //无事件　
							break;
						default:
							Logger.Trace("sync_check:{0},{1}", retcode, selector);
							break;
					}
				}
				else
				{
					Thread.Sleep(2000);
					await Schedule();
				}

				if (watch.ElapsedMilliseconds < 800)
				{
					Thread.Sleep(1000 - (int)watch.ElapsedMilliseconds);
				}
			}
		}

		private async Task HandleMessage(SyncResponse rsp)
		{
			foreach (var message in rsp.AddMsgList)
			{
				switch (message.MsgType)
				{
					case 1: //普通消息
						await SendMessage("知道了", message.FromUserName);
						break;
					case 3://图片
					case 34: //语音
					case 37://friend request
					case 42: //名片
					case 47: //表情
					case 49: //分享
					case 51:// 联系人信息
					case 62: //小视频
					case 10002:// 撤回消息
					default:
						break;

				}
			}
			//return Task.CompletedTask;
		}

		/// <summary>
		/// 计划事件
		/// </summary>
		/// <returns></returns>
		protected virtual Task Schedule()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// 测试同步检查
		/// </summary>
		/// <returns></returns>
		private async Task<bool> TestSyncCheck()
		{
			_syncHost = $"webpush.{_baseHost}";
			if ((await SyncCheck())[0] == "0")
			{
				return true;
			}
			_syncHost = $"webpush2.{_baseHost}";
			return (await SyncCheck())[0] == "0";
		}

		/// <summary>
		/// 同步检查
		/// </summary>
		/// <returns></returns>
		private async Task<string[]> SyncCheck()
		{
			var url = $"https://{_syncHost}/cgi-bin/mmwebwx-bin/synccheck?sid={_loginXml.Sid}&uin={_loginXml.Uin}&synckey={_syncKey}&r={Common.ConvertDateTimeToInt(DateTime.Now)}&skey={_loginXml.Skey}&deviceid={_loginXml.DeviceID}&_={Common.ConvertDateTimeToInt(DateTime.Now.ToUniversalTime())}";

			try
			{
				var result = await WechatHttp.Get(url);

				var match = Regex.Match(result, "window.synccheck=\\{retcode:\"(?<retcode>\\d+)\",selector:\"(?<selector>\\d+)\"\\}");
				return match.Success ? new[] { match.Groups["retcode"].Value, match.Groups["selector"].Value } : new[] { "", "" };
			}
			catch (Exception e)
			{
				Logger.Error(e);
				return new[] { "-1", "-1" };
			}
		}

		private async Task<SyncResponse> Sync()
		{
			var url = $"{_baseUri}/webwxsync?sid={_loginXml.Sid}&lang=zh_CN&skey={_loginXml.Skey}&pass_ticket={_loginXml.PassTicket}";
			var data = _loginXml.CreateSycnRequestData(_syncKey);

			var result = await WechatHttp.Post(url, data);

			var response = JsonConvert.DeserializeObject<SyncResponse>(result);
			if (response.SyncKey.Count > 0)
			{
				_syncKey = response.SyncKey;
			}

			return response;
		}

		private async Task SendMessage(string content, string to)
		{
			var url = $"{_baseUri}/webwxsendmsg?lang=zh_CN&pass_ticket={_loginXml.PassTicket}";
			var time = Common.ConvertDateTimeToInt(DateTime.Now);
			var obj = new
			{
				BaseRequest = _loginXml.GetBaseRequest(),
				Msg = new { ClientMsgId = time, Content = content, FromUserName = _me.UserName, LocalID = time, ToUserName = to, Type = 1 },
				Scene = 0
			};//1 文字消息，3 图片消息（先把图片上传得到MediaId再调用webwxsendmsg发送），其他消息类型没试
			var data = JsonConvert.SerializeObject(obj);
			var result = await WechatHttp.Post(url, data);
		}
	}



	public class User
	{
		public long Uin { get; set; }
		/// <summary>
		/// 用户名称，一个"@"为好友，两个"@"为群组
		/// </summary>
		public string UserName { get; set; }
		/// <summary>
		/// 昵称
		/// </summary>
		public string NickName { get; set; }
		/// <summary>
		/// 头像图片链接地址
		/// </summary>
		public string HeadImgUrl { get; set; }
		/// <summary>
		/// 成员数量，只有在群组信息中才有效,
		/// </summary>
		public int MemberCount { get; set; }
		/// <summary>
		/// 成员列表
		/// </summary>
		public IEnumerable<User> MemberList { get; set; }
		/// <summary>
		/// 备注名称
		/// </summary>
		public string RemarkName { get; set; }
		/// <summary>
		/// 用户名拼音缩写
		/// </summary>
		public string PYInitial { get; set; }
		/// <summary>
		/// 用户名拼音全拼
		/// </summary>
		public string PYQuanPin { get; set; }
		/// <summary>
		/// 备注拼音缩写
		/// </summary>
		public string RemarkPYInitial { get; set; }
		/// <summary>
		/// 备注拼音全拼
		/// </summary>
		public string RemarkPYQuanPin { get; set; }
		public int HideInputBarFlag { get; set; }
		/// <summary>
		/// 是否为星标朋友  0-否  1-是
		/// </summary>
		public int StarFriend { get; set; }
		/// <summary>
		/// 性别，0-未设置（公众号、保密），1-男，2-女
		/// </summary>
		public int Sex { get; set; }
		/// <summary>
		/// 公众号的功能介绍 or 好友的个性签名
		/// </summary>
		public string Signature { get; set; }
		public int AppAccountFlag { get; set; }
		public int VerifyFlag { get; set; }
		public int OwnerUin { get; set; }
		public int Statues { get; set; }
		public long AttrStatus { get; set; }
		public string Province { get; set; }
		public string City { get; set; }
		public string Alias { get; set; }
		public int UniFriend { get; set; }
		public string DisplayName { get; set; }
		public int ChatRoomId { get; set; }
		public string KeyWord { get; set; }
		public string EncryChatRoomId { get; set; }
		public int IsOwner { get; set; }

		/// <summary>
		/// 1-好友， 2-群组， 3-公众号
		/// </summary>
		public int ContactFlag { get; set; }
		public int WebWxPluginSwitch { get; set; }
		public int HeadImgFlag { get; set; }
		public int SnsFlag { get; set; }
	}

	public class SyncKey
	{
		public int Count { get; set; }
		public IEnumerable<SyncKeyList> List { get; set; } = new List<SyncKeyList>();

		public override string ToString()
		{
			return List.Aggregate("",
				(current, syncKeyList) => current + (current == "" ? "" : "%7C") + $"{syncKeyList.Key}_{syncKeyList.Val}");
		}
	}

	public class SyncKeyList
	{
		public int Key { get; set; }
		public long Val { get; set; }
	}

	public class BaseResponse
	{
		public int Ret { get; set; }
		public string ErrMsg { get; set; }
	}

	public class InitResponse
	{
		public BaseResponse BaseResponse { get; set; }
		public int Count { get; set; }
		public IEnumerable<User> ContactList { get; set; }
		public SyncKey SyncKey { get; set; }
		public User User { get; set; }
	}

	public class SyncResponse
	{
		public BaseResponse BaseResponse { get; set; }
		public SyncKey SyncKey { get; set; }
		public int AddMsgCount { get; set; }
		public IEnumerable<Message> AddMsgList { get; set; }
	}

	public class Message
	{
		public string MsgId { get; set; }
		public string FromUserName { get; set; }
		public string ToUserName { get; set; }
		public int MsgType { get; set; }
		public string Content { get; set; }
		public int Status { get; set; }
		public int ImgStatus { get; set; }
		public long CreateTime { get; set; }
		public int VoiceLength { get; set; }
		public int PlayLength { get; set; }
		public string FileName { get; set; }
		public string FileSize { get; set; }
		public string MediaId { get; set; }
		public string Url { get; set; }
		public int AppMsgType { get; set; }
		public int StatusNotifyCode { get; set; }
		public string StatusNotifyUserName { get; set; }
		public RecommendInfo RecommendInfo { get; set; }
		public int ForwardFlag { get; set; }
		public AppInfo AppInfo { get; set; }
		public int HasProductId { get; set; }
		public string Ticket { get; set; }
		public int ImgHeight { get; set; }
		public int ImgWidth { get; set; }
		public int SubMsgType { get; set; }
		public long NewMsgId { get; set; }
		public string OriContent { get; set; }
		public string EncryFileName { get; set; }
	}

	public class RecommendInfo
	{
		public string UserName { get; set; }
		public string NickName { get; set; }
		public int QQNum { get; set; }
		public string Province { get; set; }
		public string City { get; set; }
		public string Content { get; set; }
		public string Signature { get; set; }
		public string Alias { get; set; }
		public int Scene { get; set; }
		public int VerifyFlag { get; set; }
		public long AttrStatus { get; set; }
		public int Sex { get; set; }
		public string Ticket { get; set; }
		public int OpCode { get; set; }
	}

	public class AppInfo
	{
		public string AppID { get; set; }
		public int Type { get; set; }
	}

	public class StatusNotifyResponse
	{
		public BaseResponse BaseResponse { get; set; }
		public string MsgID { get; set; }
	}

	//getcontact
	public class GetContactResponse
	{
		public BaseResponse BaseResponse { get; set; }
		public int MemberCount { get; set; }
		public IEnumerable<User> MemberList { get; set; }
		public int Seq { get; set; }
	}
}

