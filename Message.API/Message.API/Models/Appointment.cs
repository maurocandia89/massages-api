namespace Message.API.Models;

public class Appointment : BaseEntity
{
    public required DateTime AppointmentDate { get; set; }
    public required Guid ClientId { get; set; }
    public ApplicationUser? Client { get; set; }
    public required Guid TreatmentId { get; set; }
    public Treatment? Treatment { get; set; }
}
