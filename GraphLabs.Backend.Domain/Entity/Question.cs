namespace Domain.Entity;

public class Question
{
    public long Id { get; set; }
    
    public string Text { get; set; } = default!;
    
    public virtual string Photo { get; set; }
    
    public virtual ICollection<TestAnswer> TestAnswers { get; set; }
    
    public long SubjectId { get; set; }
    public virtual Subject? Subject { get; set; }
    
    public virtual ICollection<TestQuestion> TestQuestions { get; set; }
    
    public virtual ICollection<TestParticipationAnswer> TestParticipationAnswers { get; set; }
}