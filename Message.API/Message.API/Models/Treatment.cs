namespace Message.API.Models;

public class Treatment : BaseEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}
