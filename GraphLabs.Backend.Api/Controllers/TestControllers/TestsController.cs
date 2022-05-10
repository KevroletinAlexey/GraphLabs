using System.Collections;
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Controllers.TestControllers.DTO;

namespace WebApplication2.Controllers.TestControllers
{
    
    [Route("[controller]")]
    [ApiController]
    public class TestsController : ODataController
    {
        private readonly GraphLabsContext _db;
        
        public TestsController(GraphLabsContext db)
        {
            _db = db;
        }
        
        
        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<TestDTO>> Get()
        {
            IQueryable<TestDTO> test = _db.Tests.Select(t => new TestDTO()
            {
                Id = t.Id,
                NameTest = t.NameTest,
                SubjectId = t.SubjectId,
                TeacherId = t.TeacherId,
                TestQuestions = t.TestQuestions.Select(tq => new TestQuestionDTO()
                {
                    Id = tq.Id,
                    difficulty = tq.difficulty,
                    SectionId = tq.SectionId,
                    Question = new QuestionDTO()
                    {
                        Id = tq.Question.Id,
                        Text = tq.Question.Text,
                        Photo = tq.Question.Photo,
                        TestAnswers = tq.Question.TestAnswers.Select(a => new TestAnswerDTO()
                        {
                            Id = a.Id,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect
                        })
                    }
                })
            });

            return Ok(test);
        }
        
        
        [HttpGet("{key:long}")]
        [EnableQuery]
        public SingleResult<TestDTO> Get(long key)
        {
            var test = _db.Tests
                .Where(t=>t.Id == key)
                .Select(t => new TestDTO()
            {
                Id = t.Id,
                NameTest = t.NameTest,
                SubjectId = t.SubjectId,
                TeacherId = t.TeacherId,
                TestQuestions = t.TestQuestions.Select(tq => new TestQuestionDTO()
                {
                    Id = tq.Id,
                    difficulty = tq.difficulty,
                    SectionId = tq.SectionId,
                    Question = new QuestionDTO()
                    {
                        Id = tq.Question.Id,
                        Text = tq.Question.Text,
                        Photo = tq.Question.Photo,
                        TestAnswers = tq.Question.TestAnswers.Select(a => new TestAnswerDTO()
                        {
                            Id = a.Id,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect
                        })
                    }
                })
            });

            return  SingleResult.Create(test);
        }
        
        
        [HttpPost]
        public async Task<ActionResult<TestDTO>> Post([FromBody] TestDTO request)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(q => q.Id == request.TeacherId);
            if (teacher == null)
            {
                return new NotFoundResult();  
            }
            var subject = await _db.Subjects.FirstOrDefaultAsync(q => q.Id == request.SubjectId);
            if (subject == null)
            {
                return new NotFoundResult();  
            }
            
            var testEntry = new Test()
            {
                NameTest = request.NameTest,
                SubjectId = request.SubjectId,
                TeacherId = request.TeacherId
            };
        
            await _db.Tests.AddAsync(testEntry);
            await _db.SaveChangesAsync();

            var id = testEntry.Id;

            foreach (var testQuestionDto in request.TestQuestions)
            {
                long idQuestion;
                
                if (testQuestionDto.Question.Id == 0)
                {
                    var questionEntry = new Question()
                    {
                        Text = testQuestionDto.Question.Text,
                        SubjectId = testQuestionDto.Question.SubjectId,
                        Photo = testQuestionDto.Question.Photo
                    };
        
                    await _db.Questions.AddAsync(questionEntry);
                    await _db.SaveChangesAsync();

                    idQuestion = questionEntry.Id;
        
                    foreach (var answerDto in testQuestionDto.Question.TestAnswers)
                    {
                        var answer = new TestAnswer()
                        {
                            Text = answerDto.Text,
                            IsCorrect = answerDto.IsCorrect,
                            QuestionId = idQuestion
                        };
                        await _db.TestAnswers.AddAsync(answer);
                    }
        
                    await _db.SaveChangesAsync();
                }
                else
                {
                    idQuestion = testQuestionDto.Question.Id;
                    
                    var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == idQuestion);
                    if (question == null)
                    {
                        return new NotFoundResult();  //возможно стоит пройтись по телу и создать вопрос, больше склоняюсь к тому что не надо
                    }
                }
                
                var testQuestion = new TestQuestion()
                {
                    difficulty = testQuestionDto.difficulty,
                    SectionId = testQuestionDto.SectionId,
                    QuestionId = idQuestion,
                    TestId = id
                };
                
                await _db.TestQuestions.AddAsync(testQuestion);
            }
            
            await _db.SaveChangesAsync();

            return new CreatedResult("testEntry", testEntry);
        }

        
        [HttpDelete("{key:long}")]
        public async Task<IActionResult> Delete(long key)
        {
            var test = await _db.Tests.SingleOrDefaultAsync(q => q.Id == key);

            if (test != null)
            {
                _db.Tests.Remove(test);
                await _db.SaveChangesAsync();
            }
            else
            {
                return new NotFoundResult();
            }

            return new NoContentResult();
        }
        
    }
}
