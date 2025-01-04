namespace DynoForm.backend.Models;

public class FormData : BaseEntity
{
    public Guid Id { get; set; }
    public required string JsonData { get; set; }

    public Form Form { get; set; }
    public Guid FormId { get; set; }
}
