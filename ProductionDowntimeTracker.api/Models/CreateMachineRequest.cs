namespace ProductionDowntimeTracker.api.Models
{
    public class CreateMachineRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
    }
}
