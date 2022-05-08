using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using NHibernate.Linq;
using WebApplication2.Controllers.TestControllers.DTO;

namespace WebApplication2.Controllers.TestControllers;

[ApiController]
[AllowAnonymous]
[Route("[controller]")]
public class TestParticipationController : ODataController
{
    private readonly GraphLabsContext _db;
        
    public TestParticipationController(GraphLabsContext db)
    {
        _db = db;
    }

    //Вернуть все назначенные тесты
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<TestParticipationSetDto>> Get()
    {
        IQueryable<TestParticipationSetDto> testParticipation = _db.TestParticipation.Select(t =>
            new TestParticipationSetDto()
            {
                Id = t.Id,
                TestId = t.TestId,
                StudentId = t.StudentId,
                DateOpen = t.DateOpen,
                DateClose = t.DateClose
            });

        return Ok(testParticipation);
    }
    
    [HttpGet("{key:long}")]
    [EnableQuery]
    public SingleResult<TestParticipationSetDto> Get(long key)
    {
        IQueryable<TestParticipationSetDto> testParticipation = _db.TestParticipation
            .Where(t=> t.Id == key)
            .Select(t =>
            new TestParticipationSetDto()
            {
                Id = t.Id,
                TestId = t.TestId,
                StudentId = t.StudentId,
                DateOpen = t.DateOpen,
                DateClose = t.DateClose
            });

        return SingleResult.Create(testParticipation);
    }


    //вернуть собранный тест для прохождения студенту, проверить что id_current_user == idStudent в testParticipation
    
    //  /TestParticipation/Test - get(key_tp) (отправить скомпанованый рандомно по секциям тест на прохождение без отметоко правильности вариантов ответа)
    //                          - post(key_tp)(принять результаты, записать в историю, посчитать балл, вернуть результат)
    
    //  /TestParticipation - get(получить краткую информацию обо всех назначенных тестах)
    //                     - get(key) (получить краткую инфу о конкретном назначении)
    //                     - delete(key) (удалить назначенный тест)
    //                     - put(key) (изменить назначенный тест)
    
    //Потом написать контроллеры для истории тестов
    //не забыть потом включить все в модель odata в programm

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TestParticipationSetDto request)
    {
        var test = await _db.Tests.FindAsync(request.TestId);
        var student = await _db.Students.FindAsync(request.StudentId);
        if ((test == null) || (student == null))
        {
            return new NotFoundResult();  
        }
        
        var testParticipationEntry = new TestParticipation()
        {
            TestId = request.TestId,
            DateOpen = request.DateOpen,
            DateClose = request.DateClose,
            StudentId = request.StudentId
        };
        
        await _db.TestParticipation.AddAsync(testParticipationEntry);
        await _db.SaveChangesAsync();
        
        return new CreatedResult("testParticipationEntry", testParticipationEntry);
    }

    [HttpPut("{key:long}")]
    public async Task<IActionResult> Put(long key, [FromBody] TestParticipationSetDto request)
    {
        if (key != request.Id)
        {
            return BadRequest();
        }
        
        var test = await _db.Tests.FindAsync(request.TestId);
        var student = await _db.Students.FindAsync(request.StudentId);
        if ((test == null) || (student == null))
        {
            return new NotFoundResult();  
        }
        
        var testParticipation = await _db.TestParticipation.FirstOrDefaultAsync(t => t.Id == key);
        
        if (testParticipation != null)
        {
            if (testParticipation.IsPassed)   // изменять назначенный тест можно, если его не проходили
            {
                return BadRequest();
            }
            
            testParticipation.TestId = request.TestId;
            testParticipation.StudentId = request.StudentId;
            testParticipation.DateOpen = request.DateOpen;
            testParticipation.DateClose = request.DateClose;
        }
        else
        {
            return new NotFoundResult();
        }
        
        return new NoContentResult();
    }
    
    [HttpDelete("{key:long}")]
    public async Task<IActionResult> Delete(long key)
    {
        var testParticipation = await _db.TestParticipation.SingleOrDefaultAsync(q => q.Id == key);

        if (testParticipation != null)
        {
            _db.TestParticipation.Remove(testParticipation);
            await _db.SaveChangesAsync();
        }
        else
        {
            return new NotFoundResult();
        }

        return new NoContentResult();
    }
    
}