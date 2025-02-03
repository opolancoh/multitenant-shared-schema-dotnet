using Microsoft.AspNetCore.Identity;
using MultitenantApiSingleDbSharedSchema.Extensions;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
// Register IHttpContextAccessor so that CurrentUserService can access HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddRepositories();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed initial admin user and role
// await app.SeedDefaultDataAsync();

app.Run();