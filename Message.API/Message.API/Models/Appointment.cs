using System.ComponentModel.DataAnnotations.Schema;
using Message.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Message.API.Models
{
    public class Appointment : BaseEntity
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
        public required Guid ClientId { get; set; }
        public ApplicationUser? Client { get; set; }
    }
}
