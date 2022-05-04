namespace Domain.Entity;

public class TestParticipationAnswer
{
    public long Id { get; set; }
    
    public long TestParticipationId { get; set; }
    public virtual TestParticipation TestParticipation { get; set; }
    
    public long QuestionId { get; set; }
    public virtual Question Question { get; set; }
    
    public long TestAnswerId { get; set; }
    public virtual TestAnswer TestAnswer { get; set; }
    
}