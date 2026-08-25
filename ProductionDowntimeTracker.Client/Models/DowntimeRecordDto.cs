namespace ProductionDowntimeTracker.Client.Models
{
    public class DowntimeRecordDto
    {
        public int Id { get; set; }

        public int MachineId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? CategoryId { get; set; }

        public string? Detail { get; set; }
    }
}