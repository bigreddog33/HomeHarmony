using Household.Api.Features.Account.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<UserIdentityDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentityApiEndpoints<ApplicationIdentityUser>(options => { options.SignIn.RequireConfirmedEmail = true; }).AddEntityFrameworkStores<UserIdentityDbContext>();
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IAccountService, MockAccountService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        var webClientOrigin = builder.Configuration["WebClientOrigin"]
            ?? "http://localhost:3000";

        policy
            .WithOrigins(webClientOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
}

app.UseCors("WebClient");
app.MapControllers();
app.Run();

// Expose the entry point to WebApplicationFactory for request-pipeline tests.
public partial class Program { }
