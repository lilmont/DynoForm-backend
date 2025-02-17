using DynoForm.backend.Models;

namespace DynoForm.backend.ViewModels;

public class FormDataListVM
{
    public string Title { get; set; }
    public List<string> Columns { get; set; }
    public List<FormFieldData> Rows { get; set; }
}
