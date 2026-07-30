namespace ProductionDowntimeTracker.Client.Models
{
    public class UpdateMachineRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public bool IsRunning { get; set; }
    }
}
