namespace Domain.Entity;

/// <summary> Модуль-задание </summary>
public class TaskModule
{
    public virtual long Id { get; set; }
        
    public virtual string Name { get; set; }
        
    public virtual string Description { get; set; }
        
    public virtual string Version { get; set; }
        
    public virtual ICollection<TaskVariant> Variants { get; set; }
}