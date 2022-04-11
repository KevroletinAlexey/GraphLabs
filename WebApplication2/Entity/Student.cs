namespace WebApplication2;

public class Student : User
{
    public virtual string Group { get; set; } = default!;
    
    public List<TestParticipation> TestParticipation { get; set; } = new();
}