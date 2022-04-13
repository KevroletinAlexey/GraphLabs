using System;
using System.Linq;
using System.Security.Claims;
using WebApplication2.DAL;
using WebApplication2.Entity;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;

namespace WebApplication2.Controllers;

public class UsersController : ODataController
{
    private readonly GraphLabsContext _db;
 
    public UsersController(GraphLabsContext context)
    {
        _db = context;
    }
         
    [HttpGet]
    [ODataRoute("currentUser")]
    [EnableQuery]
    public SingleResult<User> CurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        return SingleResult.Create(_db.Users.Where(u => u.Email == email));
    }
}