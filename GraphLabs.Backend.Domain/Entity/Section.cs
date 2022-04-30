namespace Domain.Entity;

public class Section
{
    public long Id { get; set; }
    
    public int NumberSection { get; set; }

   // public List<TestQuestion> TestQuestions { get; set; } = new();
    public virtual ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
}