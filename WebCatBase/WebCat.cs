namespace WebCatBase;

public static class WebCat
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<IEnumerable<(string Title, IEnumerable<string> Response)>> WorkAsync(
        string question,
        AiOptions aiOptions,
        Action<string, int, int>? onFetching,
        Action<string, int, int>? onProcessing)
    {
        var searchResults = (await Bing.SearchAsync(question)).ToArray();
        var current = 0;
        var results = new List<(string, IEnumerable<string>)>();
        var parseAsync = AiClient.PrepareParseAsync(aiOptions);
        foreach (var (title, url) in searchResults)
        {
            onFetching?.Invoke(title, current, searchResults.Length);
            var content = await HttpClient.GetStringAsync(url);
            onProcessing?.Invoke(title, current, searchResults.Length);
            var answers = await parseAsync(content, question);
            results.Add((title, answers));
            current++;
        }

        return results;
    }
}