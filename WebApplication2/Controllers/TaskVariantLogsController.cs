using System.Security.Claims;

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using WebApplication2.DAL;
using WebApplication2.Entity;

namespace WebApplication2.Controllers;

[ODataRoutePrefix("taskVariantLogs")]
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
        
        [EnableQuery]
        public IQueryable<TaskVariantLog> Get()
        {
            return _db.TaskVariantLogs;
        }
        
        [ODataRoute("({key})")]
        [EnableQuery]
        public SingleResult<TaskVariantLog> Get(long key)
        {
            return SingleResult.Create(_db.TaskVariantLogs.Where(v => v.Id == key));
        }
        
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Post([Microsoft.AspNetCore.Mvc.FromBody]CreateLogRequest request)
        {
            if (request == null ||
                request.VariantId == 0 ||
                string.IsNullOrEmpty(request.Action))
            {
                return PreconditionFailed();
            }

            var email = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            var student = await _db.Students.SingleOrDefaultAsync(s => s.Email == email);
            if (student == null)
                return BadRequest();
            
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

            return Created(logEntry);
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