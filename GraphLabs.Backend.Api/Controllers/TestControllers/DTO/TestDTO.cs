namespace WebApplication2.Controllers.TestControllers.DTO;

public class TestDTO
{
    public long Id { get; set; }
    public string NameTest { get; set; }
    public long SubjectId { get; set; }
    public long TeacherId { get; set; }
    public virtual IEnumerable<TestQuestionDTO> TestQuestions { get; set; }
}