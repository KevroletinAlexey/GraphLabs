namespace WebApplication2;

public class Teacher : User
{
    public List<Test> Tests { get; set; } = new();
}