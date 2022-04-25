using System.Security.Claims;
using DAL;

namespace WebApplication2.Auth;

internal sealed class UserInfoService : IUserInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserInfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor?.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.Sid);

    public string? UserRole => User?.FindFirstValue(ClaimTypes.Role);
}

//чет по поводу вопросов хз