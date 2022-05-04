namespace Domain.Entity;

public class TestParticipation
{
    public long Id { get; set; }
    
    public long TestId { get; set; }
    public virtual Test Test { get; set; }
    
    public DateTime DateOpen { get; set; }
    
    public DateTime DateClose { get; set; }
    
    public long StudentId { get; set; }
    public virtual Student Student { get; set; }
    
    public DateTime TimeStart { get; set; }
    
    public DateTime TimeFinish { get; set; }
    
    public bool IsPassed { get; set; }    //по умолчанию сделать false (вообще это флаг сдачи теста)
    
    public int Result { get; set; }     //подумать по поводу ограничений (про сложность задания, как-то надо проверять сумму баллов возможных)
    
    public virtual ICollection<TestParticipationAnswer> TestParticipationAnswers { get; set; }
    
}