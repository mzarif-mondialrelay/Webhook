var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Add this line to register controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Make sure routing is added before endpoints

app.UseAuthorization(); // Add this if you have authorization

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Ensure controllers are mapped
});

app.Run();


