module WebCat.Bing

open System
open OpenQA.Selenium

let private goToSearchPageAsync (driver: IWebDriver) (query: string) =
    driver
        .Navigate()
        .GoToUrlAsync $"https://www.bing.com/search?q={Uri.EscapeDataString query}"
    |> Async.AwaitTask

[<Struct>]
type SearchEngineResult = { Title: string; Url: string }

let private ParseSearchResult (element: IWebElement) : Option<SearchEngineResult> =
    try
        let url = element.GetAttribute "href"
        let title = element.Text
        Some { Url = url; Title = title }
    with :? StaleElementReferenceException ->
        None

let private getSearchResults (driver: IWebDriver) =
    driver.FindElements(By.CssSelector "#b_results > .b_algo h2 > a")
    |> Seq.filter _.Displayed
    |> Seq.map ParseSearchResult
    |> Seq.choose id
    |> Seq.distinct
    |> Seq.toArray

let fetchBingResultsAsync (driver: IWebDriver) (query: string)=
    async {
        do! goToSearchPageAsync driver query
        return getSearchResults driver
    }
