using Domain.Entity;

namespace WebApplication2.Controllers.TestControllers.DTO;

public sealed class QuestionDTO
{
    public long Id { get; set; }
    public string Text { get; set; }
    public long SubjectId { get; set; }
    public string Photo { get; set; }
    public IEnumerable<TestAnswerDTO> TestAnswers { get; set; }

    public QuestionDTO()
    {
        
    }

    public QuestionDTO(Question question)
    {
        Id = question.Id;
        Text = question.Text;
        SubjectId = question.SubjectId;
        Photo = question.Photo;
        TestAnswers = new List<TestAnswerDTO>();
        foreach (var answer in question.TestAnswers)
        {
            TestAnswers.Append(new TestAnswerDTO(answer));
        }
    }
}