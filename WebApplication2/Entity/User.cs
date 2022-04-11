namespace WebApplication2;

public abstract class User
{
    public virtual long Id { get; set; }
        
    public virtual string Email { get; set; } = default!;
        
    public virtual string FirstName { get; set; } = default!;
        
    public virtual string LastName { get; set; } = default!;
        
    public virtual string FatherName { get; set; } = default!;
        
    public virtual byte[] PasswordHash { get; set; } = default!;
        
    public virtual byte[] PasswordSalt { get; set; } = default!;
}