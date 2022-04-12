using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Entity;

public class Subject
{
    public long Id { get; set; }
    [Required]
    public string NameSubject { get; set; } = default!;

    public List<Test> Tests { get; set; } = new();
}