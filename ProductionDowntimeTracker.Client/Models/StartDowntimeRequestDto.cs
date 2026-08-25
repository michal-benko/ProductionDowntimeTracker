using System.ComponentModel.DataAnnotations;

namespace ProductionDowntimeTracker.Client.Models
{
    public class StartDowntimeRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vyberte stroj.")]
        public int MachineId { get; set; }
    }
}