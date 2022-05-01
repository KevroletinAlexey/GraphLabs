using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class Test
{
    public long Id { get; set; }
    
    [Required]
    public string NameTest { get; set; } = default!;
    public long SubjectId { get; set; }
    public virtual Subject Subject { get; set; }
    public long TeacherId { get; set; }
    public virtual Teacher Teacher { get; set; }

    public virtual ICollection<TestQuestion> TestQuestions { get; set; }

    public virtual ICollection<TestParticipation> TestParticipation { get; set; }
}