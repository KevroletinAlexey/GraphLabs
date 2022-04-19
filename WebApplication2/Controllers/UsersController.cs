
using System.Security.Claims;
using WebApplication2.DAL;
using WebApplication2.Entity;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers;

public class UsersController : ODataController
{
    private readonly GraphLabsContext _db;
    
    private readonly IHttpContextAccessor _contextAccessor;
 
    public UsersController(GraphLabsContext context, IHttpContextAccessor contextAccessor)
    {
        _db = context;
        _contextAccessor = contextAccessor;
    }
         
    [HttpGet]
    [ODataRoute("currentUser")]
    [EnableQuery]
    public SingleResult<User> CurrentUser()
    {
        var email = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        return SingleResult.Create(_db.Users.Where(u => u.Email == email));
    }
}