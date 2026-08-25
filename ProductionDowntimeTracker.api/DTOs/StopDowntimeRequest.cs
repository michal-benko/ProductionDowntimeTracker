using System.ComponentModel.DataAnnotations;

namespace ProductionDowntimeTracker.api.DTOs
{
    public class StopDowntimeRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Detail { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}