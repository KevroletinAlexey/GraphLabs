namespace WebApplication2.Entity;

/// <summary> Лог выполнения </summary>
public class TaskVariantLog
{
    public virtual long Id { get; set; }
        
    public virtual string Action { get; set; }
        
    public virtual DateTime DateTime { get; set; }
        
    public virtual long VariantId { get; set; }
    public virtual TaskVariant Variant { get; set; }

    public virtual long StudentId { get; set; }
    public virtual Student Student { get; set; }
        
    public int Penalty { get; set; }
}