using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers.TestControllers;

[AllowAnonymous]
[Route("[controller]")]
public class SubjectsController : ODataController
{
    
    private readonly GraphLabsContext _db;

    public SubjectsController(GraphLabsContext db)
    {
        _db = db;
    }
    
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<Subject>> Get()
    {
        IQueryable<SubjectDto> subjects = _db.Subjects.Select(s => new SubjectDto
        {
            id = s.Id,
            name = s.NameSubject
        });
        return Ok(_db.Subjects);
    }

    [HttpGet("({key})")]
    [EnableQuery]
    public SingleResult<SubjectDto> Get(long key)
    {
        IQueryable<SubjectDto> subject = _db.Subjects
            .Where(s => s.Id == key)
            .Select(s => new SubjectDto
            {
                id = s.Id,
                name = s.NameSubject
            });
        
        return SingleResult.Create(subject);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreteSubject request)
    {
        var subjectEntry = new Subject()
        {
            NameSubject = request.name
        };
        await _db.Subjects.AddAsync(subjectEntry);
        await _db.SaveChangesAsync();

        return new CreatedResult("subjectEntry", subjectEntry);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(long key)
    {
        var subject = await _db.Subjects.SingleOrDefaultAsync(s => s.Id == key);
        
        if (subject != null)
        {
            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
        }
        else
        {
            return new NotFoundResult();
        }

        return new NoContentResult();
    }

    // [HttpPatch]
    // public async Task<IActionResult> Patch(long key, [FromBody] Delta<Subject> delta)
    // {
    //     var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == key);
    //
    //     if (subject != null)
    //     {
    //         delta.Patch(subject);
    //         await _db.SaveChangesAsync();
    //     }
    //     else
    //     {
    //         return new NotFoundResult();
    //     }
    //
    //     return new NoContentResult();
    // }

    [HttpPut]
    public async Task<IActionResult> Put(long key, [FromBody] CreteSubject request)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == key);
        
        if (subject != null)
        {
            subject.NameSubject = request.name;
            await _db.SaveChangesAsync();
        }
        else
        {
            return new NotFoundResult();
        }

        return new NoContentResult();
    }

    public class SubjectDto
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class CreteSubject
    {
        public string name { get; set; }
    }
}