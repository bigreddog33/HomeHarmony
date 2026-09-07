using Household.Api.Features.Account.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
