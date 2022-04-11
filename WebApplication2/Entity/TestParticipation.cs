using System.ComponentModel.DataAnnotations;

namespace WebApplication2;

public class TestParticipation
{
    public long Id { get; set; }
    
    
    public Test? Test { get; set; }
    
    public DateTime DateOpen { get; set; }
    
    public DateTime DateClose { get; set; }
    
    
    public Student? Student { get; set; }
    
    public DateTime TimeStart { get; set; }
    
    public DateTime TimeFinish { get; set; }
    
    public bool IsPassed { get; set; }    //по умолчанию сделать false (вообще это флаг сдачи теста)
    
    public int Result { get; set; }     //подумать по поводу ограничений (про сложность задания, как-то надо проверять сумму баллов возможных)
}