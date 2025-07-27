using OpenQA.Selenium;

namespace WebCat.Browser;

public readonly record struct SearchResult(string Title, string Url)
{
    public readonly string Title = Title;
    public readonly string Url = Url;
}

public static class Bing
{
    public static class Utils
    {
        public static Task GoToSearchPageAsync(IWebDriver driver, string query) => driver
            .Navigate()
            .GoToUrlAsync($"https://www.bing.com/search?q={Uri.EscapeDataString(query)}");

        public static SearchResult[] GetSearchResults(IWebDriver driver) => driver
            .FindElements(By.CssSelector("#b_results h2 > a"))
            .Select(element =>
            {
                try
                {
                    var url = element.GetAttribute("href");
                    var title = element.GetInnerText();
                    SearchResult? searchResult = new SearchResult(title!, url!);
                    return searchResult;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            })
            .Where(element => element is not null)
            .Distinct()
            .Select(element => element!.Value)
            .ToArray();
    }

    public static async Task<SearchResult[]> FetchSearchResultsAsync(IWebDriver driver, string query)
    {
        await Utils.GoToSearchPageAsync(driver, query);
        return Utils.GetSearchResults(driver);
    }
}