using DynoForm.backend.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DynoDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var environment = builder.Environment.EnvironmentName;

        if (environment == "Development")
        {
            var devOrigins = allowedOrigins.GetSection("Development").Get<string[]>();
            policy.WithOrigins(devOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else if (environment == "Production")
        {
            var prodOrigin = Environment.GetEnvironmentVariable("MY_SERVER_IP");
            policy.WithOrigins(prodOrigin)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();
