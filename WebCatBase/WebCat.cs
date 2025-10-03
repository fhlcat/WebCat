namespace WebCatBase;

public static class WebCat
{
    private static readonly HttpClient HttpClient = new();

    public delegate void WorkEvent<in TCurrent>(int currentCount, int totalCount, TCurrent current);

    public record struct WorkEvents(
        WorkEvent<string>? OnFetchingStarted,
        WorkEvent<string>? OnFetchingCompleted,
        WorkEvent<string>? OnProcessingStarted,
        WorkEvent<string>? OnProcessingCompleted
    );

    public static async Task<IEnumerable<(string Title, IEnumerable<string> Response)>> WorkAsync(
        string question,
        AiOptions aiOptions,
        WorkEvents workEvents
    )
    {
        var searchResults = (await Bing.SearchAsync(question)).ToArray();
        var current = -1;
        var results = new List<(string, IEnumerable<string>)>();
        var parseAsync = Ai.PrepareParseAsync(aiOptions);
        foreach (var (title, url) in searchResults)
        {
            current++;
            workEvents.OnFetchingStarted?.Invoke(current, searchResults.Length,title);
            var response = await HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) continue;
            workEvents.OnFetchingCompleted?.Invoke(current, searchResults.Length, title);

            var content = await response.Content.ReadAsStringAsync();
            workEvents.OnProcessingStarted?.Invoke(current, searchResults.Length, title);
            var answers = await parseAsync(content, question);
            results.Add((title, answers));
            workEvents.OnProcessingCompleted?.Invoke(current, searchResults.Length, title);
        }

        return results;
    }
}