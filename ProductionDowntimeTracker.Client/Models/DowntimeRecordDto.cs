namespace ProductionDowntimeTracker.Client.Models
{
    public class DowntimeRecordDto
    {
        public int Id { get; set; }

        public int MachineId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
