namespace Domain.Entity;

public class Teacher : User
{
    public List<Test> Tests { get; set; } = new();
}