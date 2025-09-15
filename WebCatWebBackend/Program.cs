using System.Text.Json;
using Serilog;
using WebCatBase;

namespace WebCatWebBackend;

public static class Program
{
    private record struct TaskRequest(string Question, AiOptions AiOptions);

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("web-log.txt")
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        var tasks = new LinkedList<Work.WorkInfo>();

        app.MapPost("/tasks", (TaskRequest request) =>
        {
            var workInfo = Work.StartWork(request.Question, request.AiOptions);
            tasks.AddLast(workInfo);
            return workInfo.Id;
        });
        app.MapGet("/tasks/{id}/progress", async (HttpContext context, string id) =>
        {
            var workInfo = tasks.FirstOrDefault(info => info.Id == id);
            if (workInfo == default)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Task not found");
                await context.Response.Body.FlushAsync();
                return;
            }

            var completionTask = workInfo.ProgressReader.Completion;
            while (await workInfo.ProgressReader.WaitToReadAsync())
            {
                var progressUpdate = await workInfo.ProgressReader.ReadAsync();
                var json = JsonSerializer.Serialize(progressUpdate);
                await context.Response.WriteAsync(json);
                await context.Response.Body.FlushAsync();
            }
        });
        
        var resultsSerializationOptions = new JsonSerializerOptions
        {
            IncludeFields = true
        };
        app.MapGet("/tasks/{id}/result", (string id) =>
        {
            var workInfo = tasks.FirstOrDefault(info => info.Id == id);
            if (workInfo == default)
            {
                return Results.NotFound("Task not found");
            }

            return workInfo.Task.IsCompleted
                ? Results.Ok(JsonSerializer.Serialize(workInfo.Task.Result, resultsSerializationOptions))
                : Results.NotFound("Task not completed yet");
        });

        app.Run();
    }
}