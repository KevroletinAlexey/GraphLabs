using System.Security.Claims;
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Controllers.TestControllers.DTO;

namespace WebApplication2.Controllers.TestControllers;

//добавить потом проверку времени

[ApiController]
[Route("[controller]")]
public class TestingController : ODataController
{
    private readonly GraphLabsContext _db;
    private readonly IHttpContextAccessor _contextAccessor;
        
    public TestingController(GraphLabsContext db, IHttpContextAccessor contextAccessor)
    {
        _db = db;
        _contextAccessor = contextAccessor;
    }
    
    
    [HttpGet("{key:long}")]
    public async Task<ActionResult<TestingDTO>> Get(long key)     //ключ от TestParticipation
    {
        //доступ может получить только пользователь id которого совпадает с id в TestParticipation
        
        var email = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return new BadRequestResult();

        var student = await _db.Students.SingleOrDefaultAsync(s => s.Email == email);
        if (student == null)
            return new BadRequestResult();
        
        if (student.Id != _db.TestParticipation
                            .Where(t => t.Id == key)
                            .Select(t => t.StudentId).FirstOrDefault())
        {
            return new ForbidResult();
        }

        if (await _db.TestParticipation.Where(t => t.Id == key).Select(t => t.IsPassed).FirstOrDefaultAsync())
        {
            return new BadRequestResult();
        }
        
        var testId = await _db.TestParticipation
            .Where(t => t.Id == key)
            .Select(t=>t.Test.Id).FirstOrDefaultAsync();


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

        var sections = await testQuestions.Select(t => t.SectionId).Distinct().ToListAsync();
        
        List<TestQuestionDTO> testResult = new List<TestQuestionDTO>();
        
        foreach (var sectionId in sections)
        {
            var testQuestionsSection = testQuestions
                .Where(t => t.SectionId == sectionId).ToList();

            TestQuestionDTO testQuestionSection = testQuestionsSection[new Random().Next(0, testQuestionsSection.Count - 1)];
            testResult.Add(testQuestionSection);
        }
        

        var testing = _db.TestParticipation
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
            }).FirstOrDefault();

        if (testing == null)
        {
            return NotFound();
        }

        var testParticipation = _db.TestParticipation.FirstOrDefault(t => t.Id == key);

        if (testParticipation != null)
        {
            testParticipation.TimeStart = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        else
        {
            return NotFound();
        }

        return Ok(new[]{testing}.AsQueryable());
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TestResultDto request)
    {
        //также добавил проверку пользователя
        
        var email = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return new BadRequestResult();

        var student = await _db.Students.SingleOrDefaultAsync(s => s.Email == email);
        if (student == null)
            return new BadRequestResult();
        
        if (student.Id != _db.TestParticipation
                .Where(t => t.Id == request.TestParticipationId)
                .Select(t => t.StudentId).FirstOrDefault())
        {
            return new ForbidResult();
        }
        
        var testParticipation = await _db.TestParticipation.FirstOrDefaultAsync(t => t.Id == request.TestParticipationId);
        
        if (testParticipation != null)
        {
            testParticipation.TimeFinish = DateTime.UtcNow;
            
            //проверка времени прохождения теста
            
            var passageTime = testParticipation.TimeFinish - testParticipation.TimeStart;
            var setTime = testParticipation.DateClose - testParticipation.DateOpen;

            if (passageTime.CompareTo(setTime) == 1)
            {
                testParticipation.Score = 0;
                testParticipation.IsPassed = true;
                
                await _db.SaveChangesAsync();
                
                return BadRequest();
            }
            
            double score = 0.0;

            foreach (var question in request.Questions)
            {
                double difficulty = _db.TestQuestions
                    .Where(t => t.QuestionId == question.Id)
                    .Select(t => t.difficulty).FirstOrDefault();

                //var count = _db.TestAnswers.Count(t => t.QuestionId == question.Id);
                double numberCorrect = _db.TestAnswers
                    .Where(t => t.QuestionId == question.Id)
                    .Count(t => t.IsCorrect == true);
                double point = difficulty / numberCorrect;
            
                foreach (var answer in question.Answers)
                {
                    if (answer.isChosen)
                    {
                        var testParticipationAnswer = new TestParticipationAnswer()     //сохранить выбранные варианты ответов
                        {
                            TestParticipationId = request.TestParticipationId,
                            QuestionId = question.Id,
                            TestAnswerId = answer.Id
                        };
                        await _db.TestParticipationAnswers.AddAsync(testParticipationAnswer);
                    }

                    var isCorrect = _db.TestAnswers.Where(t => t.Id == answer.Id).Select(t => t.IsCorrect).FirstOrDefault();
                    if (isCorrect && isCorrect == answer.isChosen)
                    {
                        score += point;
                    }
                }
            
            }

            testParticipation.IsPassed = true;
            testParticipation.Score = score;
            
            await _db.SaveChangesAsync();
        }
        else
        {
            return new NotFoundResult();
        }
        
        return Ok(new {score = testParticipation.Score});
    }
    
}