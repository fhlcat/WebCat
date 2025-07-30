using OpenQA.Selenium;
using WebCat.Fetch.Struct;

namespace WebCat.Fetch.Browser;

public static class Bing
{
    public static class Utils
    {
        public static Task GoToSearchPageAsync(IWebDriver driver, string query)
        {
            return driver
                .Navigate()
                .GoToUrlAsync($"https://www.bing.com/search?q={Uri.EscapeDataString(query)}");
        }

        private static SearchEngineResult? ParseSearchResult(IWebElement element)
        {
            try
            {
                var url = element.GetAttribute("href");
                var title = element.Text;
                SearchEngineResult? searchResult = new SearchEngineResult(title, url!);
                return searchResult;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }

        public static SearchEngineResult[] GetSearchResults(IWebDriver driver) => driver
            .FindElements(By.CssSelector("#b_results > .b_algo h2 > a"))
            .Where(element => element.Displayed)
            .Select(ParseSearchResult)
            .Where(result => result is not null)
            .Distinct()
            .Select(result => result!.Value)
            .ToArray();
    }

    public static async Task<SearchEngineResult[]> FetchSearchResultsAsync(IWebDriver driver, string query)
    {
        await Utils.GoToSearchPageAsync(driver, query);
        return Utils.GetSearchResults(driver);
    }
}