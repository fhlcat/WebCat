using WebCat.Fetch.Struct;
using WebCat.Process.Struct;
using WebCat.Struct;
using static WebCat.Fetch.Browser.Bing;
using static WebCat.Fetch.Browser.Utils;
using static WebCat.Process.Utils;

namespace WebCat;

public static class Work
{
    public readonly record struct WorkEvents(
        Action<Struct.Progress<SearchEngineResult>> Fetching,
        Action<Struct.Progress<FetchResult>> Processing
    )
    {
        public readonly Action<Struct.Progress<SearchEngineResult>> Fetching = Fetching;
        public readonly Action<Struct.Progress<FetchResult>> Processing = Processing;
    }

    public readonly record struct WorkOptions(
        int Interval,
        ProcessOptions ProcessOptions,
        bool Headless,
        BrowserType BrowserType
    )
    {
        public readonly int Interval = Interval;
        public readonly ProcessOptions ProcessOptions = ProcessOptions;
        public readonly bool Headless = Headless;
        public readonly BrowserType BrowserType = BrowserType;
    }

    public readonly record struct WorkResult(FetchResult FetchResult, IEnumerable<string> ProcessResult)
    {
        public readonly FetchResult FetchResult = FetchResult;
        public readonly IEnumerable<string> ProcessResult = ProcessResult;
    }

    public record struct WorkRecord(string Query, IEnumerable<WorkResult> Results)
    {
        public readonly string Query = Query;
        public readonly IEnumerable<WorkResult> Results = Results;
    }

    public static async Task<WorkRecord> WorkAsync(
        string query,
        WorkEvents events,
        WorkOptions options
    )
    {
        using var driver = Init(options.BrowserType, options.Headless);
        var searchResults = await FetchSearchResultsAsync(driver, query);
        var totalCount = searchResults.Length;
        var processAsync = Request(options.ProcessOptions);

        var workResults = searchResults
            .MapiAsync(Fetch)
            .LoadAsync()
            .MapiAsync(Process);

        var results = await Task.Run(WorkResult[] () => workResults.ToBlockingEnumerable().ToArray());
        driver.Close();
        return new WorkRecord(query, results);

        async Task<FetchResult> Fetch(SearchEngineResult engineResult, int i)
        {
            events.Fetching(new Struct.Progress<SearchEngineResult>(engineResult, i + 1, totalCount));
            // ReSharper disable once AccessToDisposedClosure
            var webpage = await FetchWebpageAsync(driver, engineResult.Url);
            await Task.Delay(options.Interval);
            return new FetchResult(engineResult, webpage);
        }

        async Task<WorkResult> Process(FetchResult fetchingResult, int i)
        {
            events.Processing(new Struct.Progress<FetchResult>(fetchingResult, i + 1, searchResults.Length));
            var aiResult = await processAsync(new ProcessRequest(fetchingResult.Webpage.Content, query));
            return new WorkResult(fetchingResult, aiResult);
        }
    }
}