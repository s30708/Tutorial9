using Tutorial9.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddOpenApi();
builder.Services.AddScoped<IDbService, DbService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
