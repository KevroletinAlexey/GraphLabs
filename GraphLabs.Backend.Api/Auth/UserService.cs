using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DAL;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication2.Auth;

public sealed class UserService
    {
        private readonly GraphLabsContext _ctx;
        private readonly PasswordHashCalculator _hashCalculator;
        private readonly AuthSettings _appSettings;

        public UserService(IOptions<AuthSettings> appSettings, GraphLabsContext ctx, PasswordHashCalculator hashCalculator)
        {
            _ctx = ctx;
            _hashCalculator = hashCalculator;
            _appSettings = appSettings.Value;
        }

        private LoginResponse CreateLoggedInResponse(User user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Sid, user.Id.ToString(CultureInfo.InvariantCulture)),
                    new Claim(ClaimTypes.Role, user.GetType().Name),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new LoginResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenHandler.WriteToken(token)
            };
        }
        
        public async Task<LoginResponse> Authenticate(LoginRequest login)
        {
            var user = await _ctx.Users.SingleOrDefaultAsync(x => x.Email == login.Email);
            if (user == null)
                return null;
            
            var hash = _hashCalculator.Calculate(login.Password, user.PasswordSalt);
            if (!hash.SequenceEqual(user.PasswordHash))
                return null;


            return CreateLoggedInResponse(user);
        }
        
        public async Task<LoginResponse> Renew(ClaimsPrincipal principal)
        {
            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return null;
            
            var user = await _ctx.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return null;

            return CreateLoggedInResponse(user);
        }
    }