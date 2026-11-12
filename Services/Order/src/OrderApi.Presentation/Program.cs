using OrderApi.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerConfig();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HardwareNexus Order Api v1");
    }
);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();