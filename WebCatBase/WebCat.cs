namespace WebCatBase;

public static class WebCat
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<IEnumerable<(string Title, IEnumerable<string> Response)>> WorkAsync(
        string question,
        AiOptions aiOptions,
        Action<string, int, int>? onFetching,
        Action<string, int, int>? onProcessing
    )
    {
        var searchResults = (await Bing.SearchAsync(question)).ToArray();
        var current = -1;
        var results = new List<(string, IEnumerable<string>)>();
        var parseAsync = Ai.PrepareParseAsync(aiOptions);
        foreach (var (title, url) in searchResults)
        {
            current++;
            onFetching?.Invoke(title, current, searchResults.Length);
            var response = await HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) continue;
            
            var content = await response.Content.ReadAsStringAsync();
            onProcessing?.Invoke(title, current, searchResults.Length);
            var answers = await parseAsync(content, question);
            results.Add((title, answers));
        }

        return results;
    }
}