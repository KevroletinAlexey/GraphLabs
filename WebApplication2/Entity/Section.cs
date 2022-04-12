namespace WebApplication2.Entity;

public class Section
{
    public long Id { get; set; }
    
    public int NumberSection { get; set; }

    public List<TestQuestion> TestQuestions { get; set; } = new();
}