using System.Threading.Channels;
using Serilog;
using WebCatBase;

namespace WebCatWebBackend;

public static class Work
{
    public interface IProgressUpdate
    {
        public string Message { get; }
        public int Current { get; }
        public int Total { get; }
    }

    private record struct FetchingProgressUpdate(string Message, int Current, int Total) : IProgressUpdate;

    private record struct ProcessingProgressUpdate(string Message, int Current, int Total) : IProgressUpdate;

    public record struct WorkInfo(
        string Id,
        Task<IEnumerable<(string Title, IEnumerable<string> Response)>> Task,
        ChannelReader<IProgressUpdate> ProgressReader
    );

    public static WorkInfo StartWork(string question, AiOptions aiOptions)
    {
        var progressChannel = Channel.CreateUnbounded<IProgressUpdate>();
        var task = WebCat
            .WorkAsync(question, aiOptions, OnFetching, OnProcessing)
            .ContinueWith(workingTask =>
            {
                progressChannel.Writer.Complete();
                return workingTask.Result;
            });
        var id = Guid.NewGuid().ToString();
        return new WorkInfo(id, task, progressChannel.Reader);

        void OnFetching(string title, int current, int total)
        {
            Log.Information("Fetching {Title} - {Current}/{Total}", title, current, total);
            progressChannel.Writer.TryWrite(new FetchingProgressUpdate($"Fetching started: {title}", current, total));
        }

        void OnProcessing(string title, int current, int total)
        {
            Log.Information("Processing {Title} - {Current}/{Total}", title, current, total);
            progressChannel.Writer.TryWrite(
                new ProcessingProgressUpdate($"Processing started: {title}", current, total)
            );
        }
    }
}