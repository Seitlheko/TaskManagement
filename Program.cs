using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskManagement.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MariaDbServerVersion(new Version(10, 4, 32))
    ));


var app = builder.Build();
app.Urls.Add("http://+:5069");
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var retries = 10;
    while (retries > 0)
    {
        try
        {
            dbContext.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"Retrying DB connection... attempts left: {retries}");
            Thread.Sleep(3000); // wait 3 seconds
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseDefaultFiles(); // Enables index.html default
app.UseStaticFiles();  // Serves wwwroot/www/etc.

app.MapControllers();

app.Run();
