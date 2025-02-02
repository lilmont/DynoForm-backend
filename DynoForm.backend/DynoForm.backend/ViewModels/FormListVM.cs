namespace DynoForm.backend.ViewModels;

public class FormListVM
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedDate { get; set; } 
    public DateTime LastModifiedDate { get; set; }
}

public class FormListResponseViewModel
{
    public int TotalCount { get; set; }
    public List<FormListVM> Forms { get; set; }
}
