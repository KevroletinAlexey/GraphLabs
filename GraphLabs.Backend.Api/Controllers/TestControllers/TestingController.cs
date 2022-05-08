using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using WebApplication2.Controllers.TestControllers.DTO;

namespace WebApplication2.Controllers.TestControllers;

[ApiController]
[AllowAnonymous]
[Route("[controller]")]
public class TestingController : ODataController
{
    private readonly GraphLabsContext _db;
        
    public TestingController(GraphLabsContext db)
    {
        _db = db;
    }
    
    
    [HttpGet("{key:long}")]
    public SingleResult<TestingDTO> Get(long key)     //ключ от TestParticipation
    {
        
        var testId = _db.TestParticipation
            .Where(t => t.Id == key)
            .Select(t=>t.Test.Id).FirstOrDefault();


        var testQuestions = _db.TestQuestions
            .Where(t => t.TestId == testId)
            //.OrderBy(t => t.SectionId)
            .Select(q => new TestQuestionDTO()
            {
                Id = q.Id,
                difficulty = q.difficulty,
                SectionId = q.SectionId,
                Question = new QuestionDTO()
                {
                    Id = q.QuestionId,
                    Text = q.Question.Text,
                    SubjectId = q.Question.SubjectId,
                    Photo = q.Question.Photo,
                    TestAnswers = q.Question.TestAnswers.Select(a => new TestAnswerDTO()
                    {
                        Id = a.Id,
                        Text = a.Text
                    })
                }
            });

        var sections = testQuestions.Select(t => t.SectionId).Distinct().ToList();
        
        List<TestQuestionDTO> testResult = new List<TestQuestionDTO>();
        
        foreach (var sectionId in sections)
        {
            var testQuestionsSection = testQuestions
                .Where(t => t.SectionId == sectionId).ToList();

            TestQuestionDTO testQuestionSection = testQuestionsSection[new Random().Next(0, testQuestionsSection.Count - 1)];
            testResult.Add(testQuestionSection);
        }
        

        IQueryable<TestingDTO> testing = _db.TestParticipation
            .Where(t => t.Id == key)
            .Select(t => new TestingDTO()
            {
                Id = t.Id,
                StudentId = t.StudentId,
                DateOpen = t.DateOpen,
                DateClose = t.DateClose,
                Test = new TestDTO()
                {
                    Id = t.Test.Id,
                    NameTest = t.Test.NameTest,
                    SubjectId = t.Test.SubjectId,
                    TeacherId = t.Test.TeacherId,
                    TestQuestions = testResult
                }
            });
        
        return SingleResult.Create(testing);
    }
}