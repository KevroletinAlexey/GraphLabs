using System.Collections;
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers.TestControllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    //[ApiController]
    public class TestsController : ODataController
    {
        private readonly GraphLabsContext _db;


        public TestsController(GraphLabsContext db)
        {
            _db = db;
        }
        
        //
        // [HttpGet]
        // [EnableQuery]
        // public ActionResult<IQueryable> Get()
        // {
        //     var tests = _db.Tests
        //         .Select(t => new
        //         {
        //             id = t.Id,
        //             subject = t.SubjectId,
        //             name = t.NameTest,
        //             teacherId = t.TeacherId,
        //             questions = t.TestQuestions.Select(q => new
        //             {
        //                 id = q.Id,
        //                 section = q.SectionId,
        //                 text = q.Text,
        //                 photo = q.Photo,
        //                 difficulty = q.difficulty,
        //                 answers = q.TestAnswers.Select(a => new
        //                 {
        //                     id = a.Id,
        //                     text = a.Text
        //                 })
        //             })
        //         });
        //
        //     return Ok(tests);
        // }

        // [HttpGet("({key}")]
        // [EnableQuery]
        // public SingleResult Get(long key)
        // {
        //     var test = _db.Tests
        //         .Where(t => t.Id == key)
        //         .Select(t => new
        //         {
        //             id = t.Id,
        //             subject = t.SubjectId,
        //             name = t.NameTest,
        //             teacherId = t.TeacherId,
        //             questions = t.TestQuestions.Select(q => new
        //             {
        //                 id = q.Id,
        //                 section = q.SectionId,
        //                 text = q.Question.
        //                 //photo = q.Photo,
        //                 difficulty = q.difficulty,
        //                 
        //                 
        //                 
        //                 answers = q.TestAnswers.Select(a => new
        //                 {
        //                     id = a.Id,
        //                     text = a.Text
        //                 })
        //             })
        //         });
        //
        //     return SingleResult.Create(test);
        // }



        // нужны ли veiwModels?
        public class ViewTest
        {
            public long id { get; set; }
            public string name { get; set; }
            public virtual ICollection<long> questions { get; set; }
        }
        
    }
}
