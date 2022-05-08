namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestParticipationSetDto
{
    public long Id { get; set; }
    public long TestId { get; set; }
    public DateTime DateOpen { get; set; }
    public DateTime DateClose { get; set; }
    public long StudentId { get; set; }
}