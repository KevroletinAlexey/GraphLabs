
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace WebApplication2.Controllers;

[Route("students")]
//[ODataRoutePrefix("students")]
public class StudentsController : ODataController
{
    private readonly GraphLabsContext _db;

    public StudentsController(GraphLabsContext context)
    {
        _db = context;
    }
    
    [HttpGet]
    [EnableQuery]
    public IQueryable<Student> Get()
    {
        return _db.Students;
    }
        
    [HttpGet("({key})")]
    [EnableQuery]
    public SingleResult<Student> Get(long key)
    {
        return SingleResult.Create(_db.Students.Where(v => v.Id == key));
    }
}