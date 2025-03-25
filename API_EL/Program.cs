using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;

var builder = WebApplication.CreateBuilder(args);

// ✅ Сначала добавляем сервисы (контроллеры, Swagger, SQL)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Elgaria API", Version = "v1" });
});

// ✅ Только теперь создаём `app`
var app = builder.Build();

// ✅ Включаем Swagger, если среда разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elgaria API v1"));
}

// ✅ Подключение к MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    using var connection = new MySqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("✅ Подключение к MySQL успешно!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Urls.Add("http://0.0.0.0:" + Environment.GetEnvironmentVariable("PORT"));
app.Run();
