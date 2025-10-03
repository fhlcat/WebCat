using Serilog;

namespace WebCatWebBackend;

public static class Program
{

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("web-log.txt")
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        
        var taskController = new TaskController();

        app.MapPost("/tasks", taskController.CreateTask);
        app.MapGet("/tasks/{id}/progress", taskController.GetTaskProgress);
        app.MapGet("/tasks/{id}/result", taskController.GetTaskResult);

        app.Run();
    }
}