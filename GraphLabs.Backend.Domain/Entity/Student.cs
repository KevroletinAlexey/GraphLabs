using System.Linq.Expressions;

namespace Domain.Entity;

public class Student : User
{
    public virtual string Group { get; set; } = default!;
    
    public virtual ICollection<TestParticipation> TestParticipation { get; set; }

    public virtual ICollection<TaskVariantLog> Logs { get; set; }
    
}