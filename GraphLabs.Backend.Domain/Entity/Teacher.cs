namespace Domain.Entity;

public class Teacher : User
{
    public virtual ICollection<Test> Tests { get; set; }
}