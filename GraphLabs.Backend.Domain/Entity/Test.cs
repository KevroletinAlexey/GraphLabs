using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class Test
{
    public long Id { get; set; }
    
    [Required]
    public string NameTest { get; set; } = default!;
    
    public Subject? Subject { get; set; }
    
    public Teacher? Teacher { get; set; }

    public List<TestQuestion> TestQuestions { get; set; } = new();

    public List<TestParticipation> TestParticipation { get; set; } = new();
}