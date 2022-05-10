using System.Collections;
using System.Security.Claims;
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using NHibernate.Criterion;
using WebApplication2.Controllers.TestControllers.DTO;

namespace WebApplication2.Controllers.TestControllers;


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

        TestingDTO? testing = new TestingDTO();
        
        var testParticipation = _db.TestParticipation.FirstOrDefault(t => t.Id == key);
        if (testParticipation == null) return NotFound();
        
        //тест отправляем первый раз, то формируем

        var isSending = await _db.TestParticipation.Where(t => t.Id == key).Select(t => t.IsSending).FirstOrDefaultAsync();
        
        if (!isSending)
        {
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
            
            //сохранение варианта в TestParticioationAnswer

            foreach (var question in testResult)
            {
                var testParticipationAnswer = new TestParticipationAnswer()     
                {
                    TestParticipationId = key,
                    QuestionId = question.Id
                };
                await _db.TestParticipationAnswers.AddAsync(testParticipationAnswer);
            }
            
            testing = _db.TestParticipation
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
            

            if (testParticipation != null)
            {
                testParticipation.TimeStart = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }

            //return Ok(new[]{testing}.AsQueryable());
            
        }
        else  //если возвращаем тест с отмеченными вариантами ответов
        {
            //проверка времени
            var setTime = testParticipation.DateClose - testParticipation.DateOpen;
            var passageTime = DateTime.UtcNow + TimeSpan.FromMinutes(1) - testParticipation.TimeStart;  //добавил минуту, может и не надо
            if (passageTime.CompareTo(setTime) > 0)
            {
                return BadRequest();
            }

            long[] questions = _db.TestParticipationAnswers
                .Where(t => t.TestParticipationId == key)
                //.Where(t=>t.TestAnswerId != null)
                //.Distinct()
                .Select(t=>t.QuestionId).ToArray();
            
            var questions2 = _db.TestParticipationAnswers
                .Where(t => t.TestParticipationId == key)
                //.Where(t=>t.TestAnswerId != null)
                //.Distinct()
                .Select(t=> new TestParticipationAnswer()
                {
                    QuestionId = t.QuestionId,
                    TestAnswerId = t.TestAnswerId
                })
                .ToArray();
            
            Console.Write(questions);
          
            
            
            
            testing = _db.TestParticipation
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
                        TestQuestions = t.Test.TestQuestions
                            
                           // .Where(q=>q.QuestionId.IsIn(new ArrayList(questions)))
                            
                            .Select(q=> new TestQuestionDTO()
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
                                        Text = a.Text,
                                        //IsChosen = (questions2.Select(q=>q.TestAnswerId).Contains(a.Id)) ? true : false 
                                    })
                                }
                            })
                    }
                }).FirstOrDefault();
            
            
            //выбираем назначенные раннее вопросы

            var pq = _db.TestParticipationAnswers
                .Where(t => t.TestParticipationId == key)
                .Select(t => t.QuestionId)
                .Distinct();

            foreach (var p in pq)
            {
                Console.WriteLine(p);
            }
            
            List<TestQuestionDTO> resultTestQuestions = new List<TestQuestionDTO>();

            foreach (var testQuestion in testing.Test.TestQuestions)
            {
                
                if (pq.Any(q => q == testQuestion.Question.Id))
                {
                    resultTestQuestions.Add(testQuestion);
                }
            }

            testing.Test.TestQuestions = resultTestQuestions;
            
            //отмечаем выбранные ответы
            foreach (var question in testing.Test.TestQuestions)
            {
                var tpa = _db.TestParticipationAnswers
                    .Where(t => t.TestParticipationId == key && t.QuestionId == question.Question.Id);
                foreach (var answer in question.Question.TestAnswers)
                {
                    if (tpa.Any(t => t.TestAnswerId == answer.Id))
                    {
                        answer.IsChosen = true;
                    }
                }
            }
            
            //return Ok(testing2);
        }
        
        return Ok(new[]{testing}.AsQueryable());
        
        
        // var testParticipation = _db.TestParticipation.FirstOrDefault(t => t.Id == key);
        //
        // if (testParticipation != null)
        // {
        //     testParticipation.TimeStart = DateTime.UtcNow;
        //     await _db.SaveChangesAsync();
        // }
        // else
        // {
        //     return NotFound();
        // }
        //
        // return Ok(new[]{testing}.AsQueryable());
    }

    
    //[HttpPost]
    [HttpPut("{key:long}")]
    public async Task<IActionResult> Post(long key, [FromBody] TestResultDto request)
    {
        if (key != request.TestParticipationId)
        {
            return BadRequest();
        }
        
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
        var testParticipationAnswers = _db.TestParticipationAnswers.Where(t => t.TestParticipationId == request.TestParticipationId);
        
        if (testParticipation != null)
        {
            testParticipation.TimeFinish = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            
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
            
            //подсчет результатов
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

                int countChosenAnswer = 0;
                
                foreach (var answer in question.Answers)
                {
                    if (answer.isChosen)
                    {
                        countChosenAnswer += 1;

                        if (countChosenAnswer == 1)
                        {
                            var testQuestionVariant = await testParticipationAnswers.Where(t => t.QuestionId == question.Id).FirstOrDefaultAsync();
                        
                            if (testQuestionVariant != null)
                            {
                                testQuestionVariant.TestAnswerId = answer.Id;
                                await _db.SaveChangesAsync();
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                        else
                        {
                            var testParticipationAnswer = new TestParticipationAnswer()     //сохранить выбранные варианты ответов
                            {
                                TestParticipationId = request.TestParticipationId,
                                QuestionId = question.Id,
                                TestAnswerId = answer.Id
                            };
                            await _db.TestParticipationAnswers.AddAsync(testParticipationAnswer);
                        }
                    }

                    var isCorrect = _db.TestAnswers.Where(t => t.Id == answer.Id).Select(t => t.IsCorrect).FirstOrDefault();
                    if (isCorrect && isCorrect == answer.isChosen)
                    {
                        score += point;
                    }
                }    //созранение ответов и подсчет баллов
            
            }

            testParticipation.IsPassed = request.IsPassed;  //флаг отправки ответа студентом
            testParticipation.IsSending = true;             //флаг принятия результатов, нобязательно конечных
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
