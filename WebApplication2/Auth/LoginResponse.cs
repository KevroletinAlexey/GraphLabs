namespace WebApplication2.Auth;

public class LoginResponse
{
    public long UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Token { get; set; }
}