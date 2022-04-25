
using System.Security.Claims;

using DAL;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
         
    //[HttpGet]
    // [ODataRoute("currentUser")]
    [HttpGet("currentUser")]
    [EnableQuery]
    public SingleResult<User> CurrentUser()
    {
        var email = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        return SingleResult.Create(_db.Users.Where(u => u.Email == email));
    }
}