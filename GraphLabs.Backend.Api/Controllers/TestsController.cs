using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace WebApplication2.Controllers
{
    [Route("[controller]")]
    //[ApiController]
    public class TestsController : ControllerBase
    {
        private readonly GraphLabsContext _db;


        public TestsController(GraphLabsContext db)
        {
            _db = db;
        }
        

        [HttpGet("[action]")]
        [EnableQuery]
        public ActionResult<IQueryable<Test>> GetAll()
        {
            IQueryable<Test> tests = _db.Tests;

            return Ok(tests);
        }
    }
}
