namespace Domain.Entity;

public class TestAnswer
{
    public long Id { get; set; }
    
    public string Text { get; set; } = default!;
    
    public bool IsCorrect { get; set; }
    public long TestQuestionId { get; set; }
    public virtual TestQuestion TestQuestion { get; set; }
}