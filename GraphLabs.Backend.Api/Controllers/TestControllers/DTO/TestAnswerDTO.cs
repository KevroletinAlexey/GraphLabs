using System.Security.Cryptography.X509Certificates;
using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestAnswerDTO
{
    public long Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
    
    public bool IsChosen { get; set; }
    
    public TestAnswerDTO() {}

    public TestAnswerDTO(TestAnswer testAnswer)
    {
        Id = testAnswer.Id;
        Text = testAnswer.Text;
        IsCorrect = testAnswer.IsCorrect;
    }
}