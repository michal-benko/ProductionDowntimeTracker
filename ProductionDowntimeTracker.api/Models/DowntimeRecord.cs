namespace ProductionDowntimeTracker.api.Models
{
    public class DowntimeRecord
    {
        public int Id { get; set; }

        public int MachineId { get; set; }

        public DateTime StartTime { get; set; }

        //null znamená, že prostoj stále probíhá
        public DateTime? EndTime { get; set; }

        
        public string Reason { get; set; } = string.Empty;

        public Machine Machine { get; set; } = null!;

    }
}
