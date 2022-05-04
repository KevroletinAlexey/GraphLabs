namespace Domain.Entity;

public class TestAnswer
{
    public long Id { get; set; }
    
    public string Text { get; set; } = default!;
    
    public bool IsCorrect { get; set; }
    
    public long QuestionId { get; set; }
    public virtual Question Question { get; set; }
    
    public virtual ICollection<TestParticipationAnswer> TestParticipationAnswers { get; set; }
}