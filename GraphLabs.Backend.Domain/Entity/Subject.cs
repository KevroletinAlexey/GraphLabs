using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class Subject
{
    public long Id { get; set; }
    [Required]
    public string NameSubject { get; set; } = default!;

    //public List<Test> Tests { get; set; } = new();

    public virtual ICollection<Test> Tests { get; set; }
    
    public virtual ICollection<Question> Questions { get; set; }
}