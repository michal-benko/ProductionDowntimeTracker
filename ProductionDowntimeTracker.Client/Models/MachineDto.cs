namespace ProductionDowntimeTracker.Client.Models
{
    public class MachineDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsRunning { get; set; }
    }
}
