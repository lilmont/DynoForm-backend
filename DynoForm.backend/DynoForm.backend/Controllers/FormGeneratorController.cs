using DynoForm.backend.Context;
using DynoForm.backend.Models;
using DynoForm.backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace DynoForm.backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class FormGeneratorController : Controller
{
    private readonly DynoDbContext _dbContext;
    public FormGeneratorController(DynoDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [HttpPost("add")]
    public async Task<IActionResult> AddForm([FromBody] FormVM form)
    {
        if (form == null || string.IsNullOrEmpty(form.JsonSchema))
            return BadRequest("Invalid request.");

        try
        {
            Guid newId = Guid.NewGuid();
            // Parse the JSON schema
            var newForm = new Form
            {
                Id = newId,
                Title = form.Title,
                JsonSchema = form.JsonSchema,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            _dbContext.Forms.Add(newForm);
            await _dbContext.SaveChangesAsync();

            return Ok(new { FormId = newId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetForm([Required] Guid Id)
    {
        if (Id == new Guid())
            return BadRequest("Invalid request.");

        try
        {
            var form = await _dbContext.Forms.FirstOrDefaultAsync(p => p.Id == Id);

            if (form == null)
                return NotFound();

            return Ok(new { Form = form });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("edit")]
    public async Task<IActionResult> EditForm([FromBody] FormVM form)
    {
        if (form == null || string.IsNullOrEmpty(form.JsonSchema))
            return BadRequest("Invalid request.");

        try
        {
            var existingForm = await _dbContext.Forms.FirstOrDefaultAsync(p => p.Id == form.Id);

            if (existingForm == null)
                return NotFound();

            // Parse the JSON schema
            existingForm.Title = form.Title;
            existingForm.JsonSchema = form.JsonSchema;
            existingForm.DateUpdated = DateTime.UtcNow;

            _dbContext.Forms.Update(existingForm);
            await _dbContext.SaveChangesAsync();

            return Ok(new { FormId = form.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetFormList()
    {
        try
        {
            var totalForms = await _dbContext.Forms.CountAsync();

            var forms = await _dbContext.Forms
                                         .Select(f => new FormListVM
                                         {
                                             Id = f.Id,
                                             Title = f.Title,
                                             CreatedDate = f.DateCreated,
                                             LastModifiedDate = f.DateUpdated
                                         })
                                         .ToListAsync();


            var response = new FormListResponseViewModel
            {
                TotalCount = totalForms,
                Forms = forms
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("add-data")]
    public async Task<IActionResult> AddFormData([FromBody] FormDataVM formData)
    {
        if (formData == null || string.IsNullOrEmpty(formData.JsonData))
            return BadRequest("Invalid request.");

        try
        {
            Guid newId = Guid.NewGuid();
            var newFormData = new FormData
            {
                Id = newId,
                JsonData = formData.JsonData,
                FormId = formData.FormId,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.FormData.Add(newFormData);

                await SaveFormFieldDataAsync(newId, newFormData);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { FormDataId = newId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("form-data-details")]
    public async Task<IActionResult> GetFormData([Required] Guid Id)
    {
        if (Id == new Guid())
            return BadRequest("Invalid request.");

        try
        {
            var formData = await _dbContext.FormData.FirstOrDefaultAsync(p => p.Id == Id);

            if (formData == null)
                return NotFound();

            return Ok(new { FormData = formData });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("edit-data")]
    public async Task<IActionResult> EditFormData([FromBody] FormDataVM formData)
    {
        if (formData == null || string.IsNullOrEmpty(formData.JsonData))
            return BadRequest("Invalid request.");

        try
        {
            var existingFormData = await _dbContext.FormData
                .FirstOrDefaultAsync(p => p.Id == formData.Id);

            if (existingFormData == null)
                return NotFound();

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Update the FormData
                existingFormData.JsonData = formData.JsonData;
                existingFormData.DateUpdated = DateTime.UtcNow;
                _dbContext.FormData.Update(existingFormData);

                await SaveFormFieldDataAsync(existingFormData.Id, existingFormData);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { FormDataId = existingFormData.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private async Task SaveFormFieldDataAsync(Guid formDataId, FormData formData)
    {
        // Remove existing FormFieldData records
        var existingFields = await _dbContext.FormFieldData
            .Where(f => f.FormDataId == formDataId)
            .ToListAsync();

        if (existingFields.Any())
        {
            _dbContext.FormFieldData.RemoveRange(existingFields);
        }

        // Extract new FormFieldData
        var formFieldData = ExtractFormFieldData(formData);
        if (formFieldData.Count > 0)
        {
            _dbContext.FormFieldData.AddRange(formFieldData);
        }
    }


    private List<FormFieldData> ExtractFormFieldData(FormData formData)
    {
        try
        {
            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(formData.JsonData);

            if (jsonObject != null)
            {
                var fieldDataEntries = jsonObject.Select(kvp => new FormFieldData
                {
                    Id = Guid.NewGuid(),
                    FormDataId = formData.Id,
                    FieldKey = kvp.Key,
                    FieldValue = kvp.Value.ValueKind switch
                    {
                        JsonValueKind.String => kvp.Value.GetString() ?? string.Empty,
                        JsonValueKind.Number => kvp.Value.GetRawText() ?? string.Empty,
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Array => string.Join(", ", kvp.Value.EnumerateArray().Select(e => e.ToString())),
                        JsonValueKind.Object => kvp.Value.ToString(),
                        _ => string.Empty
                    },
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                }).ToList();

                return fieldDataEntries;
            }

            return new List<FormFieldData>();
        }
        catch
        {
            return new List<FormFieldData>();
        }
    }

    [HttpGet("form-data-list")]
    public async Task<IActionResult> GetAllFormDataByFormId([Required] Guid Id)
    {
        try
        {
            var existingFormData = await _dbContext.FormFieldData
                .Where(p => p.FormData.FormId == Id)
                .ToListAsync();

            var formTitle = await _dbContext.Forms
                .Where(p => p.Id == Id)
                .Select(p => p.Title)
                .FirstOrDefaultAsync();


            var result = new FormDataListVM
            {
                Title = formTitle != null ? formTitle : string.Empty,
                Columns = existingFormData.GroupBy(p => p.FieldKey).Select(p => p.Key).ToList(),
                Rows = existingFormData
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
