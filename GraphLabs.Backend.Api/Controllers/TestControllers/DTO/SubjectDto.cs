using Domain.Entity;
using Microsoft.AspNetCore.OData.Results;

namespace WebApplication2.Controllers.TestControllers.DTO;

public class SubjectDto
{
        
    public long id { get; set; }
    public string name { get; set; }
    
    public SubjectDto() {}

    public SubjectDto(Subject subject)
    {
        id = subject.Id;
        name = subject.NameSubject;
    }
}