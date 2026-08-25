using System.ComponentModel.DataAnnotations;

namespace ProductionDowntimeTracker.Client.Models
{
    public class StopDowntimeRequestDto
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vyberte kategorii.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Detail prostoje je povinný.")]
        [StringLength(
            500,
            MinimumLength = 5,
            ErrorMessage = "Detail musí obsahovat 5 až 500 znaků.")]
        public string Detail { get; set; } = string.Empty;
    }
}