using System.ComponentModel.DataAnnotations;

namespace Message.API.Models;

public class Appointment : BaseEntity
{
    [Required]
    public string ClientName { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    public string? Description { get; set; }
}
