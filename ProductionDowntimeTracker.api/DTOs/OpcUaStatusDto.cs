namespace ProductionDowntimeTracker.api.DTOs
{
    public class OpcUaStatusDto
    {
        public bool MachineRunning { get; set; }
        public bool FaultActive { get; set; }
        public uint ProducedCycles { get; set; }
    }
}
