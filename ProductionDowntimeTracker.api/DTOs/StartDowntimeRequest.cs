using System.ComponentModel.DataAnnotations;

namespace ProductionDowntimeTracker.api.DTOs
{
    public class StartDowntimeRequest
    {
        [Range(1, int.MaxValue)]
        public int MachineId { get; set; }
    }
}
