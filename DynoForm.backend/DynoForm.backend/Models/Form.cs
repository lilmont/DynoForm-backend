namespace DynoForm.backend.Models;

public class Form : BaseEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string JsonSchema { get; set; }
}
