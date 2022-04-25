namespace Domain.Entity;

public class Student : User
{
    public virtual string Group { get; set; } = default!;
    
    public List<TestParticipation> TestParticipation { get; set; } = new();
    
    public virtual ICollection<TaskVariantLog> Logs { get; set; }
}