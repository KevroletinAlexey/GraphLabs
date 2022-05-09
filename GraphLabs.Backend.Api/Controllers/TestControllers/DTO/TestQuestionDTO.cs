using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestQuestionDTO
{
    public long Id { get; set; }
    public int difficulty { get; set; }
    public long SectionId { get; set; }
    
    public QuestionDTO Question { get; set; }

    public TestQuestionDTO()
    {
        
    }

    public TestQuestionDTO(TestQuestion testQuestion)
    {
        Id = testQuestion.Id;
        difficulty = testQuestion.difficulty;
        SectionId = testQuestion.SectionId;
        Question = new QuestionDTO(testQuestion.Question);
    }
}
