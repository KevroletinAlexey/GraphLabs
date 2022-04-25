using System.Security.Claims;

using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers;

//[ODataRoutePrefix("taskVariantLogs")]
[Route("taskVariantLogs")]
    public class TaskVariantLogsController : ODataController
    {
        private readonly GraphLabsContext _db;
        private readonly IHttpContextAccessor _contextAccessor;

        public TaskVariantLogsController(GraphLabsContext context,
            IHttpContextAccessor contextAccessor)
        {
            _db = context;
            _contextAccessor = contextAccessor;
        }
        
        [HttpGet] //??
        [EnableQuery]
        public IQueryable<TaskVariantLog> Get()
        {
            return _db.TaskVariantLogs;
        }
        
        //[ODataRoute("({key})")]
        [HttpGet("({key})")]
        [EnableQuery]
        public SingleResult<TaskVariantLog> Get(long key)
        {
            return SingleResult.Create(_db.TaskVariantLogs.Where(v => v.Id == key));
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([Microsoft.AspNetCore.Mvc.FromBody]CreateLogRequest request)
        {
            if (request == null ||
                request.VariantId == 0 ||
                string.IsNullOrEmpty(request.Action))
            {
                return PreconditionFailed();
            }

            var email = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return new BadRequestResult();

            var student = await _db.Students.SingleOrDefaultAsync(s => s.Email == email);
            if (student == null)
                return new BadRequestResult();
            
            var logEntry = new TaskVariantLog
            {
                Action = request.Action,
                DateTime = DateTime.Now,
                StudentId = student.Id,
                VariantId = request.VariantId,
                Penalty = request.Penalty ?? 0
            };

            await _db.TaskVariantLogs.AddAsync(logEntry);
            await _db.SaveChangesAsync();

            return new CreatedResult("logEntry", logEntry);
        }

        public class CreateLogRequest
        {
            public string Action { get; set; }
        
            public long VariantId { get; set; }
            
            public int? Penalty { get; set; }
        }

        
        private StatusCodeResult PreconditionFailed()
        {
            return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);
        }
            
    }