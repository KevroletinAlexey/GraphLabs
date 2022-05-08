using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestingDTO
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public DateTime DateOpen { get; set; }
    public DateTime DateClose { get; set; }
    public TestDTO Test { get; set; }
}