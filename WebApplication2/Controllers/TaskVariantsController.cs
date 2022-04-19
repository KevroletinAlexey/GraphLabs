using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.DAL;
using WebApplication2.Entity;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers;

[ODataRoutePrefix("taskVariants")]
public class TaskVariantsController : ODataController
{
    private readonly GraphLabsContext _db;
    private readonly TaskVariantConverter _taskVariantConverter;

    public TaskVariantsController(GraphLabsContext context, TaskVariantConverter taskVariantConverter)
    {
        _db = context;
        _taskVariantConverter = taskVariantConverter;
    }
        
    [EnableQuery]
    public IQueryable<TaskVariant> Get()
    {
        return _db.TaskVariants;
    }
        
    public async Task<IActionResult> Json(long key)
    {
        var json = await _db.TaskVariants.Where(v => v.Id == key).Select(TaskVariantConverter.ToJsonExpression).SingleAsync();
        return new ContentResult
        {
            StatusCode = StatusCodes.Status200OK,
            ContentType = "application/json",
            Content = json
        };
    }
        
    [HttpPost]
    [ODataRoute("({key})")]
    public async Task<IActionResult> Post(long key)
    {
        
        
        string json;
        await using (Stream stream = await Request.Content?.ReadAsStreamAsync()!)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                json = await sr.ReadToEndAsync();
            }
        }
        
        var variant = await _taskVariantConverter.CreateOrUpdate(key, json);

        return new OkObjectResult(variant.Id);
    }
        
    [HttpDelete]
    [ODataRoute("({key})")]
    public async Task<IActionResult> Delete(long key)
    {
        var variant = await _db.TaskVariants.SingleOrDefaultAsync(v => v.Id == key);
        if (variant != null)
        {
            _db.TaskVariants.Remove(variant);
            await _db.SaveChangesAsync();
        }
        else
        {
            return new NotFoundResult();
        }

        return new OkResult();
    }
}