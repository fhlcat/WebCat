namespace WebCat;

public class WebCat(AiClient aiClient)
{
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// title, current, total
    /// </summary>
    public event Action<string, int, int>? OnFetching;

    /// <summary>
    /// title, current, total
    /// </summary>
    public event Action<string, int, int>? OnProcessing;

    public async Task<IEnumerable<(string Title, IEnumerable<string> Response)>> WorkAsync(string question)
    {
        var searchResults = (await Bing.SearchAsync(question)).ToArray();
        var current = 0;
        var results = new List<(string, IEnumerable<string>)>();
        foreach (var (title,url) in searchResults)
        {
            OnFetching?.Invoke(title, current, searchResults.Length);
            var content = await _httpClient.GetStringAsync(url);
            OnProcessing?.Invoke(title, current, searchResults.Length);
            var answers = await aiClient.ParseAsync(question, content, 0);
            results.Add((title, answers));
            current++;
        }
        return results;
    }
}