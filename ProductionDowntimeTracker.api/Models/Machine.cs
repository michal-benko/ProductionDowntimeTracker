namespace ProductionDowntimeTracker.api.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRunning { get; set; }

    }
}


