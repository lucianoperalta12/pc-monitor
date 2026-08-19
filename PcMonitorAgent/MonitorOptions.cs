namespace PcMonitorAgent;

public class MonitorOptions
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public int HeartbeatSeconds { get; set; } = 30;
}
