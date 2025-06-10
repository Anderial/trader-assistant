using DistributedKit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Trader Assistant API", 
        Version = "v1",
        Description = "API для автономного торгового ассистента с ML-анализом криптовалют"
    });
    
    // Добавляем XML комментарии для Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Добавляем Orleans Client с Rant.DistributedKit
builder.Host.UseDistributedKitClient("trader-assistant-api");

// Настройка CORS для frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trader Assistant API v1");
        c.RoutePrefix = "swagger"; // Swagger UI будет доступен по /swagger
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.MapControllers();

Console.WriteLine("=== TraderAssistant API (Orleans Client) ===");
Console.WriteLine("API starting on https://localhost:7216");

app.Run();
