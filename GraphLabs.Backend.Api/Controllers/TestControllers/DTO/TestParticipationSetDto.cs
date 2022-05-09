using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestParticipationSetDto
{
    public long Id { get; set; }
    public long TestId { get; set; }
    public DateTime DateOpen { get; set; }
    public DateTime DateClose { get; set; }
    public long StudentId { get; set; }
    
    public bool IsPassed { get; set; }

    public TestParticipationSetDto(TestParticipation testParticipation)
    {
        this.Id = testParticipation.Id;
        this.TestId = testParticipation.TestId;
        this.DateOpen = testParticipation.DateOpen;
        this.DateClose = testParticipation.DateClose;
        this.StudentId = testParticipation.StudentId;
        this.IsPassed = testParticipation.IsPassed;
    }

    public TestParticipationSetDto()
    {
        
    }
}