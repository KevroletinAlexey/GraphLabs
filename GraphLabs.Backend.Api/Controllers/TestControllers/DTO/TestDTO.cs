using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public sealed class TestDTO
{
    public long Id { get; set; }
    public string NameTest { get; set; }
    public long SubjectId { get; set; }
    public long TeacherId { get; set; }
    public IEnumerable<TestQuestionDTO> TestQuestions { get; set; }

    public TestDTO()
    {
        
    }

    public TestDTO(Test test)
    {
        Id = test.Id;
        NameTest = test.NameTest;
        SubjectId = test.SubjectId;
        TeacherId = test.TeacherId;

        List<TestQuestionDTO> questions = new List<TestQuestionDTO>();

        foreach (var question in test.TestQuestions)
        {
            questions.Add(new TestQuestionDTO(question));
        }
        
        TestQuestions = questions;
    }
}