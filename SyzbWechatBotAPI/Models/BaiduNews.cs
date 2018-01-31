using System;

namespace SyzbWechatBotAPI.Models
{
    public class BaiduNews
    {
        public long Id { get; set; }
        public string Keyword { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Html { get; set; }
        public DateTime CreateTime { get; set; }
        public string Time { get; set; }
        public string Text { get; set; }
        public string Summary { get; set; }
    }
}
