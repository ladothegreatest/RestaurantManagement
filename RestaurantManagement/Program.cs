using RestaurantManagement.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureDatabase();
builder.AddServices();
builder.AddJWTAuthentication();
builder.ConfigureSwagger();
builder.ConfigureLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
