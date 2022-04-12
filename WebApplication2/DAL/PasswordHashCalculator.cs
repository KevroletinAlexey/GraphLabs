using System.Security.Cryptography;

namespace WebApplication2.DAL;

public sealed class PasswordHashCalculator
{
    private const int Iterations = 10000;
    private const int HashSize = 128;
        
    public byte[] Calculate(string password, byte[] salt)
    {
        return new Rfc2898DeriveBytes(password, salt, Iterations).GetBytes(HashSize); 
    }
}