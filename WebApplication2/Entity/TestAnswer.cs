using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Entity;

public class TestAnswer
{
    public long Id { get; set; }
    
    public string Text { get; set; } = default!;
    
    public bool IsCorrect { get; set; }
    
    public TestQuestion? TestQuestion { get; set; }
}