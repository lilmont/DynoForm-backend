using DynoForm.backend.Context;
using DynoForm.backend.Models;
using DynoForm.backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
            // Parse the JSON schema
            var newFormData = new FormData
            {
                Id = newId,
                JsonData = formData.JsonData,
                FormId = formData.FormId,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            _dbContext.FormData.Add(newFormData);
            await _dbContext.SaveChangesAsync();

            return Ok(new { FormDataId = newId });
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
            var existingFormData = await _dbContext.FormData.FirstOrDefaultAsync(p => p.Id == formData.Id);

            if (existingFormData == null)
                return NotFound();

            // Parse the JSON schema
            existingFormData.JsonData = formData.JsonData;
            existingFormData.DateUpdated = DateTime.UtcNow;

            _dbContext.FormData.Update(existingFormData);
            await _dbContext.SaveChangesAsync();

            return Ok(new { FormDataId = existingFormData.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
