using DynoForm.backend.Models;

namespace DynoForm.backend.ViewModels;

public class FormDataVM
{
    public Guid Id { get; set; }
    public required string JsonData { get; set; }
    public Guid FormId { get; set; }
}
