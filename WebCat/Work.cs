using System.Threading.Channels;
using WebCat.Browser;

namespace WebCat;

public readonly record struct Progress<T>(T Current, int Completed, int Total)
{
    public readonly T Current = Current;
    public readonly int Completed = Completed;
    public readonly int Total = Total;

    public float Percentage => Total == 0 ? 0 : (float)Completed / Total * 100;

    public override string ToString() => $"{Completed}/{Total} completed";
}

public static class Work
{
    public readonly record struct FetchingResult(SearchResult SearchResult, Webpage Webpage)
    {
        public readonly SearchResult SearchResult = SearchResult;
        public readonly Webpage Webpage = Webpage;
    };

    public readonly record struct WorkOptions(int Interval, string Endpoint, string ApiKey, string Model)
    {
        public readonly int Interval = Interval;
        public readonly string Endpoint = Endpoint;
        public readonly string ApiKey = ApiKey;
        public readonly string Model = Model;
    }

    public readonly record struct WorkingEvents(
        Action<Progress<SearchResult>> Fetching,
        Action<Progress<Webpage>> Processing
    )
    {
        public readonly Action<Progress<SearchResult>> Fetching = Fetching;
        public readonly Action<Progress<Webpage>> Processing = Processing;
    }

    public readonly record struct WorkResult(FetchingResult FetchingResult, string[] Result)
    {
        public readonly FetchingResult FetchingResult = FetchingResult;
        public readonly string[] Result = Result;
    }

    private static async Task<IEnumerable<TO>> MapAsync<T, TO>(this IAsyncEnumerable<T> source,
        Func<T, Task<TO>> mapFunc)
    {
        var results = new List<TO>();
        await foreach (var item in source)
        {
            results.Add(await mapFunc(item));
        }

        return results;
    }

    public static async Task<IEnumerable<WorkResult>> WorkAsync(string query, WorkingEvents events, WorkOptions options)
    {
        var driver = Utils.Init();
        var searchResults = await Bing.FetchSearchResultsAsync(driver, query);

        var channel = Channel.CreateUnbounded<FetchingResult>();

        var totalCount = searchResults.Length;
        _ = Task.Run(async () =>
        {
            var completedCount = 0;

            foreach (var searchResult in searchResults)
            {
                events.Fetching(new Progress<SearchResult>(searchResult, completedCount, totalCount));
                var webpage = await Utils.FetchWebpageAsync(driver, searchResult.Url);
                await channel.Writer.WriteAsync(new FetchingResult(searchResult, webpage));
                completedCount++;
                await Task.Delay(options.Interval);
            }
        });

        var processByAiAsync = Ai.Request(new Ai.Options(options.Model, options.Endpoint, options.ApiKey));
        var processingCompletedCount = 0;
        return await channel.Reader
            .ReadAllAsync()
            .MapAsync(async fetchingResult =>
            {
                events.Processing(new Progress<Webpage>(
                    fetchingResult.Webpage,
                    processingCompletedCount,
                    searchResults.Length
                ));
                processingCompletedCount++;
                var aiResult = await processByAiAsync(
                    new AiRequest(fetchingResult.Webpage.Content, query)
                );
                return new WorkResult(fetchingResult, aiResult);
            });
    }
}