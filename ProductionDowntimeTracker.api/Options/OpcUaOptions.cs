namespace ProductionDowntimeTracker.api.Options
{
    public class OpcUaOptions
    {
        public const string SectionName = "OpcUa";

        public string EndpointUrl { get; set; } = string.Empty;
        public string MachineRunningNodeId { get; set; } = string.Empty;
        public string FaultActiveNodeId { get; set; } = string.Empty;
        public string ProducedCyclesNodeId { get; set; } = string.Empty;
    }
}
