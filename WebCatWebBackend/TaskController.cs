using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WebCatBase;

namespace WebCatWebBackend;

[ApiController]
public class TasksController: ControllerBase
{
    private readonly ConcurrentDictionary<string, Task> _tasks = new();

    public record struct TaskCreationRequest(string Question, AiOptions AiOptions);

    public event Action<int, int, string>? OnProgressUpdate;

    [HttpPost("/tasks")]
    public string CreateTask(TaskCreationRequest request)
    {
        var workEvents = new WebCat.WorkEvents(
            (count, totalCount, current) =>
                Log.Information("Fetching Started {Title} - {Current}/{Total}", count, totalCount, current),
            (count, totalCount, current) => Log.Information("Fetching Completed {Title} - {Current}/{Total}", count,
                totalCount, current),
            (count, totalCount, current) => Log.Information("Processing Started {Title} - {Current}/{Total}", count,
                totalCount, current),
            (count, totalCount, current) => Log.Information("Processing Completed {Title} - {Current}/{Total}", count,
                totalCount, current));
        var taskId = Guid.NewGuid().ToString();
        workEvents.OnProcessingCompleted +=
            (count, totalCount, _) => OnProgressUpdate?.Invoke(count, totalCount, taskId);
        var task = WebCat.WorkAsync(request.Question, request.AiOptions, workEvents);
        _tasks[taskId] = task;
        return taskId;
    }

    private readonly JsonSerializerOptions _resultsSerializationOptions = new()
    {
        IncludeFields = true
    };

    [HttpGet("/tasks/{id}/result")]
    public IResult GetTaskResult(string id) => _tasks.TryGetValue(id, out var task)
        ? Results.Ok(JsonSerializer.Serialize(task, _resultsSerializationOptions))
        : Results.NotFound("Task not completed yet");

    [HttpGet("/tasks{id}/progress")]
    public async Task GetTaskProgress(HttpContext context, string id)
    {
        var success = _tasks.TryGetValue(id, out var task);
        if (!success)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Task not found");
            await context.Response.Body.FlushAsync();
            return;
        }

        OnProgressUpdate += ReportProgress;

        if (task != null) await task;
        OnProgressUpdate -= ReportProgress;
        return;

        // ReSharper disable once AsyncVoidMethod
        async void ReportProgress(int current, int total, string taskId)
        {
            if (taskId != id) return;
            var json = JsonSerializer.Serialize(new { current, total }); 
            await context.Response.WriteAsync(json); 
            await context.Response.Body.FlushAsync();
        }
    }
}