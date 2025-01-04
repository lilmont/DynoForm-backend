namespace DynoForm.backend.Models;

public class FormFieldData : BaseEntity
{
    public Guid Id { get; set; }
    public required string FieldKey { get; set; }
    public required string FieldValue { get; set; }

    public FormData FormData { get; set; }
    public Guid FormDataId { get; set; }
}
