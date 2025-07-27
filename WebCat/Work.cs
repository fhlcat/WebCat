using WebCat.Ai;
using WebCat.Web;
using static WebCat.Web.Browser.Bing;
using static WebCat.Web.Browser.Utils;

namespace WebCat;

public static class Work
{
    public readonly record struct WorkEvents(
        Action<Progress<SearchResult>> Fetching,
        Action<Progress<Webpage>> Processing
    )
    {
        public readonly Action<Progress<SearchResult>> Fetching = Fetching;
        public readonly Action<Progress<Webpage>> Processing = Processing;
    }

    public readonly record struct WorkResult(FetchResult FetchResult, IEnumerable<string> ProcessResult)
    {
        public readonly FetchResult FetchResult = FetchResult;
        public readonly IEnumerable<string> ProcessResult = ProcessResult;
    }

    public static async Task<WorkResult[]> WorkAsync(
        string query,
        WorkEvents events,
        WorkOptions options
    )
    {
        using var driver = Init(options.BrowserType, options.Headless);
        var searchResults = await FetchSearchResultsAsync(driver, query);
        var totalCount = searchResults.Length;
        var processByAiAsync = Ai.Utils.Request(new Ai.Utils.Options(options.Model, options.Endpoint, options.ApiKey));

        var processResult = await searchResults
            .MapiAsync(Fetch)
            .Preload()
            .MapiAsync(Process)
            .ToEnumerableAsync();

        return [.. processResult];

        async Task<FetchResult> Fetch(SearchResult result, int i)
        {
            events.Fetching(new Progress<SearchResult>(result, i + 1, totalCount));
            // ReSharper disable once AccessToDisposedClosure
            var webpage = await FetchWebpageAsync(driver, result.Url);
            await Task.Delay(options.Interval);
            return new FetchResult(result, webpage);
        }

        async Task<WorkResult> Process(FetchResult fetchingResult, int i)
        {
            events.Processing(new Progress<Webpage>(fetchingResult.Webpage, i + 1, searchResults.Length));
            var aiResult = await processByAiAsync(new AiRequest(fetchingResult.Webpage.Content, query));
            return new WorkResult(fetchingResult, aiResult);
        }
    }
}