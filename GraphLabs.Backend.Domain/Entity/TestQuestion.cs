namespace Domain.Entity;

public class TestQuestion
{
    public long Id { get; set; }
    
    public string Text { get; set; } = default!;
    
    public virtual string Photo { get; set; }
    
    public int difficulty { get; set; } 
    
    public virtual Section Section { get; set; }
    
    public virtual Test Test { get; set; }

    public virtual ICollection<TestAnswer> TestAnswers { get; set; }
}