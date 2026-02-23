using DWIS.RigOS.Common.Worker;

namespace DWIS.Service.DownholeECD.Model
{
    public class ConfigurationForDownholeECD : Configuration
    {
        public bool EnableRealtimeDataDump { get; set; } = true;
        public string RealtimeDataDumpDirectory { get; set; } = "/home";
        public TimeSpan RealtimeDataDumpInterval { get; set; } = TimeSpan.FromHours(1);
    }
}
