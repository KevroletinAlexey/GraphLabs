using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Controllers.TestControllers.DTO;

namespace WebApplication2.Controllers.TestControllers;

[ApiController]
[AllowAnonymous]
[Route("[controller]")]
public class QuestionController : ODataController
{
    private readonly GraphLabsContext _db;

    public QuestionController(GraphLabsContext db)
    {
        _db = db;
    }

    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<QuestionDTO>> Get()
    {
        IQueryable<QuestionDTO> questions = _db.Questions.Select(q => new QuestionDTO
        {
            Id = q.Id,
            Text = q.Text,
            SubjectId = q.SubjectId,
            Photo = q.Photo,
            TestAnswers = q.TestAnswers.Select(a=> new TestAnswerDTO
            {
                Id = a.Id,
                Text = a.Text,
                IsCorrect = a.IsCorrect
            })
        });

        return Ok(questions);
    }
    
    [HttpGet("{key:long}")]
    [EnableQuery]
    public SingleResult<QuestionDTO> Get(long key)
    {
        var question = _db.Questions
            .Where(q => q.Id == key)
            .Select(q => new QuestionDTO
        {
            Id = q.Id,
            Text = q.Text,
            SubjectId = q.SubjectId,
            Photo = q.Photo,
            TestAnswers = q.TestAnswers.Select(a=> new TestAnswerDTO
            {
                Id = a.Id,
                Text = a.Text,
                IsCorrect = a.IsCorrect
            })
        });
        
        return SingleResult.Create(question);
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] QuestionDTO request)
    {
        var questionEntry = new Question()
        {
            Text = request.Text,
            SubjectId = request.SubjectId,
            Photo = request.Photo
        };
        
        await _db.Questions.AddAsync(questionEntry);
        await _db.SaveChangesAsync();

        var id = questionEntry.Id;
        
        foreach (var answerDto in request.TestAnswers)
        {
            var answer = new TestAnswer()
            {
                Text = answerDto.Text,
                IsCorrect = answerDto.IsCorrect,
                QuestionId = id
            };
            await _db.TestAnswers.AddAsync(answer);
        }
        
        await _db.SaveChangesAsync();

        return new CreatedResult("questionEntry", questionEntry);
    }
    
    [HttpDelete("{key:long}")]
    public async Task<IActionResult> Delete(long key)
    {
        var question = await _db.Questions.SingleOrDefaultAsync(q => q.Id == key);

        if (question != null)
        {
            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
        }
        else
        {
            return new NotFoundResult();
        }

        return new NoContentResult();
    }
    
    [HttpPut("{key:long}")]
    public async Task<IActionResult> Put(long key, [FromBody] QuestionDTO request)
    {
        if (key != request.Id)
        {
            return BadRequest();
        }
        
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == key);
        
        if (question != null)
        {
            question.Text = request.Text;
            question.Photo = request.Photo;
            question.SubjectId = request.SubjectId;
            
            await _db.SaveChangesAsync();

            foreach (var requestTestAnswer in request.TestAnswers)
            {
                var answer = await _db.TestAnswers.FirstOrDefaultAsync(t => t.Id == requestTestAnswer.Id);
                if (answer != null)
                {
                    answer.Text = requestTestAnswer.Text;
                    answer.IsCorrect = requestTestAnswer.IsCorrect;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    new NotFoundResult();
                }
            }
        }
        else
        {
            return new NotFoundResult();
        }
        
        return new NoContentResult();
    }
    
}