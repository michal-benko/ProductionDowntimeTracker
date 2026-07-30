using System.ComponentModel.DataAnnotations;

namespace ProductionDowntimeTracker.Client.Models
{
    public class CreateMachineRequestDto
    {
        [Required(ErrorMessage = "Název stroje je povinný.")]
        [MaxLength(100, ErrorMessage = "Název může mít maximálně 100 znaků.")]
        public string Name { get; set; } = string.Empty;

        public bool IsRunning { get; set; }
    }
}
