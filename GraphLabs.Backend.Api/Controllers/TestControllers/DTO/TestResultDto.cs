namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestResultDto
{
    public long TestParticipationId { get; set; }
    public bool IsPassed { get; set; }
    public virtual IEnumerable<QuestionCheck> Questions { get; set; }
}

public class QuestionCheck
{
    public long Id { get; set; }
    public virtual IEnumerable<AnswerCheck> Answers { get; set; }
}

public class AnswerCheck
{
   public long Id { get; set; }
   public bool isChosen { get; set; }
}