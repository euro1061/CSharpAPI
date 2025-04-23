using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QBackend API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "ใส่ JWT Token ตรงนี้ เช่น: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors((options) =>
    {
        options.AddPolicy("DevCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        options.AddPolicy("ProdCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("https://myProductionSite.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
    });

string? tokenKeyString = builder.Configuration.GetSection("AppSettings:TokenKey").Value;

SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : ""));

TokenValidationParameters validationParameters = new TokenValidationParameters()
{
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = key
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = validationParameters;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.MapOpenApi();
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
