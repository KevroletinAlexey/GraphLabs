using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public class QuestionDTO
{
    public long Id { get; set; }
    public string Text { get; set; }
    public long SubjectId { get; set; }
    public virtual string Photo { get; set; }
    public virtual IEnumerable<TestAnswerDTO> TestAnswers { get; set; }
    
}