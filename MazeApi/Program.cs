using MazeApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();
app.UseCors();
app.MapGet("/api/algorithms", () => new[] { "astar", "greedy", "dijkstra" });
app.MapHub<MazeHub>("/hub/maze");
app.Run();