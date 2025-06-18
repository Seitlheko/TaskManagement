using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskManagement.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TaskManagement API", Version = "v1" });

    // Add JWT auth to Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MariaDbServerVersion(new Version(10, 4, 32))
    ));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_dev_key";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
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

            var hasher = new System.Security.Cryptography.SHA256Managed();
            string adminEmail = "admin@task.local";
            string rawPassword = "admin123";

            if (!dbContext.Users.Any(u => u.Email == adminEmail))
            {
                string hashed = Convert.ToBase64String(
                       hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawPassword))
);

                dbContext.Users.Add(new TaskManagement.Models.User
                {
                    Name = "System Admin",
                    Email = adminEmail,
                    Role = "Admin",
                    PasswordHash = hashed,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });


                dbContext.SaveChanges();
                Console.WriteLine("Seeded admin: admin@task.local / admin123");
            }

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManagement API V1");
    c.RoutePrefix = "swagger"; // Serve at /swagger/
});


//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles(); // Enables index.html default
app.UseStaticFiles();  // Serves wwwroot/www/etc.

app.MapControllers();

app.Run();
