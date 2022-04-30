using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers
{
    [Route("[controller]")]
    //[ApiController]
    public class TestsController : ODataController
    {
        private readonly GraphLabsContext _db;


        public TestsController(GraphLabsContext db)
        {
            _db = db;
        }
        
        [AllowAnonymous]
        [HttpGet("[action]")]
        [EnableQuery]
        public ActionResult<IQueryable<Test>> GetAll()
        {
            IQueryable<Test> tests = _db.Tests
                .Include(t=>t.Subject)
                .Include(t=>t.Teacher)
                .Include(t=>t.TestQuestions)
                    .ThenInclude(question => question.TestAnswers)
                .AsNoTracking();

            var test2 = _db.Tests
                .Join(
                    _db.Subjects,
                    test => test.Subject!.Id,
                    subject => subject.Id,
                    (test, subject) => new
                    {
                        id = test.Id,
                        nameTest = test.NameTest,
                        subject = new
                        {
                            id = subject.Id,
                            nameSubject = subject.NameSubject
                        }
                        // teacher = _db.Tests.Join(_db.Teachers,
                        //     test => test.Teacher!.Id,
                        //     teacher => teacher.Id, (test1, teacher) => new
                        //     {
                        //         id = teacher.Id
                        //     })
                    });

            var result = from test in _db.Tests
                join teacher in _db.Teachers on test.Teacher!.Id equals teacher.Id
                join subject in _db.Subjects on test.Subject!.Id equals subject.Id
                join question in _db.TestQuestions on test.Id equals question.Test.Id
                select new
                {
                    id = test.Id,
                    nameTest = test.NameTest,
                    teacherId = teacher.Id,
                    subject = new
                    {
                        id = subject.Id,
                        nameSubject = subject.NameSubject
                    },
                    question = new
                    {
                        id = question.Id,
                        text = question.Text,
                        photo = question.Photo,
                        difficulty = question.difficulty
                    }
                };
            
            //var res = from tes in _db.Te
            
            return Ok(result);
        }
    }
}
