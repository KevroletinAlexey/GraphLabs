namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestQuestionDTO
{
    public long Id { get; set; }
    public int difficulty { get; set; }
    public long SectionId { get; set; }
    
    public QuestionDTO Question { get; set; }
}