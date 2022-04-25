namespace Domain.Entity;

public class TestQuestion
{
    public long Id { get; set; }
    
    public string Text { get; set; } = default!;
    
    public string? Photo { get; set; }
    
    public int difficulty { get; set; } 
    
    public Section? Section { get; set; }
    
    public Test? Test { get; set; }

    public List<TestAnswer> TestAnswers { get; set; } = new();
}