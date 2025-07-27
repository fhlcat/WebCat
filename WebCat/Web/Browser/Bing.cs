using OpenQA.Selenium;

namespace WebCat.Web.Browser;

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

        private static SearchResult? ParseSearchResult(IWebElement element)
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
        }

        public static SearchResult[] GetSearchResults(IWebDriver driver)
        {
            return [.. driver
                .FindElements(By.CssSelector("#b_results > .b_algo h2 > a"))
                .Select(ParseSearchResult)
                .Where(element => element is not null)
                .Distinct()
                .Select(element => element!.Value)];
        }
    }

    public static async Task<SearchResult[]> FetchSearchResultsAsync(IWebDriver driver, string query)
    {
        await Utils.GoToSearchPageAsync(driver, query);
        return Utils.GetSearchResults(driver);
    }
}