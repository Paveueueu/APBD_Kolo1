using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Kolokwium1.Repositories;
using Kolokwium1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IExampleRepository, ExampleRepository>();
builder.Services.AddScoped<IExampleService, ExampleService>();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kolokwium API",
        Version = "v1",
        Description = "",
        Contact = new OpenApiContact
        {
            Name = "s30660",
            Email = "s30660@pjwstk.edu.pl",
            Url = new Uri("https://github.com/Paveueueu")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kolokwium API");
        c.DocExpansion(DocExpansion.List);
        c.DefaultModelExpandDepth(0);
        c.DisplayRequestDuration();
        c.EnableFilter();
    }
);

app.UseAuthorization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            
            _ => StatusCodes.Status500InternalServerError
        };

        var result = JsonSerializer.Serialize(new { error = exception?.Message });
        await context.Response.WriteAsync(result);
    });
});

app.MapControllers();

app.Run();