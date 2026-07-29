namespace ProductionDowntimeTracker.api.Models
{
    public class UpdateMachineRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
    }
}
