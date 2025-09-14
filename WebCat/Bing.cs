using HtmlAgilityPack;

namespace WebCat;

public static class Bing
{
    private static readonly HttpClient Client = new();

    /// <returns>The title and url of the results</returns>
    public static async Task<IEnumerable<KeyValuePair<string, string>>> SearchAsync(string query)
    {
        var response = await Client.GetStringAsync($"https://www.bing.com/search?q={Uri.EscapeDataString(query)}");
        var doc = new HtmlDocument();
        doc.LoadHtml(response);
        return doc.DocumentNode
            .SelectNodes("//li[@class='b_algo']//h2/a")
            .Select(node => new KeyValuePair<string, string>(
                node.InnerText.Trim(),
                node.GetAttributeValue("href", "")
            ))
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value));
    }
}