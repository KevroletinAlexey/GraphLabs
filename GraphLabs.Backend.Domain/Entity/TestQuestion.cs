namespace Domain.Entity;

public class TestQuestion
{
    public long Id { get; set; }
    
    public int difficulty { get; set; }
    
    public long SectionId { get; set; }
    public virtual Section Section { get; set; }
    
    public long TestId { get; set; }
    public virtual Test Test { get; set; }
    
    public long QuestionId { get; set; }
    public virtual Question Question { get; set; }
}

 