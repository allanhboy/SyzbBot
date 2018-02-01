using System;

namespace SyzbWechatBotAPI.Models
{
    public class Monitor
    {
        public Guid Id { get; set; }
        public MonitorType Type { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remarks { get; set; }
        public int NewsCount { get; set; }
    }
}
