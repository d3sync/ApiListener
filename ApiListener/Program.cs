using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .WriteTo.Console()
    .WriteTo.File("logs/ApiListenerLog.txt", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer(); // This is required for Swagger

// Add Swagger generator
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Listener", Version = "v1" });
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Listener V1"));



app.UseStaticFiles();


app.UseHttpsRedirection();

app.MapMethods("{*path}", new[] { "GET", "POST", "PUT", "DELETE" }, async (HttpContext context) =>
{
    string path = context.Request.Path;

    // Check if the path starts with "/swagger", indicating a Swagger-related request
    if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
    {
        // Do not process Swagger requests; let them pass through
        context.Response.StatusCode = 404; // You might want to simply not handle this path.
        ;
    }
    string method = context.Request.Method;
    string bodyContent = await new StreamReader(context.Request.Body).ReadToEndAsync();

    // Log the incoming request details
    Log.Information("Received {Method} request on {Path}. Body: {Body}", method, path, bodyContent);

    // Here, you can add your logic to handle the request
    // For simplicity, we are just returning a success message.
    return Results.Ok(new { StatusCode = 200, message = "Request logged successfully.", Request = bodyContent });
});

app.Run();
