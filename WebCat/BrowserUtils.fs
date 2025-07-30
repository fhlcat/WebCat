module WebCat.BrowserUtils

open System
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Edge

type Browser =
    | Chrome
    | Edge

let private argumentsBase =
    [| "--disable-blink-features=AutomationControlled"
       "--disable-extensions"
       // ReSharper disable once StringLiteralTypo
       "--disable-infobars"
       "--disable-notifications"
       "--disable-popup-blocking"
       "--disable-web-security"
       "--ignore-certificate-errors"
       "--no-sandbox"
       "--start-maximized"
       "--disable-background-networking"
       "--disable-default-apps"
       // ReSharper disable once StringLiteralTypo
       "--enable-unsafe-swiftshader"
       "--mute-audio" |]

let private excludedArguments = [| "enable-automation"; "useAutomationExtension" |]

let initWebDriver (browser: Browser, headless: bool) : IWebDriver =
    let arguments =
        if headless then
            Array.append argumentsBase [| "--headless" |]
        else
            argumentsBase

    match browser with
    | Chrome ->
        let service = ChromeDriverService.CreateDefaultService()
#if !DEBUG
        service.HideCommandPromptWindow <- true
#endif
        let options = ChromeOptions()
        options.AddArguments arguments
        options.AddExcludedArguments excludedArguments
        options.PageLoadStrategy <- PageLoadStrategy.Eager
        options.ImplicitWaitTimeout <- TimeSpan.FromSeconds 10.0

        new ChromeDriver(service, options)

    | Edge ->
        Environment.SetEnvironmentVariable("SE_DRIVER_MIRROR_URL", "https://msedgedriver.microsoft.com")

        let service = EdgeDriverService.CreateDefaultService()
#if !DEBUG
        service.HideCommandPromptWindow <- true
#endif
        let options = EdgeOptions()
        options.AddArguments arguments
        options.AddExcludedArguments excludedArguments
        options.PageLoadStrategy <- PageLoadStrategy.Eager
        options.ImplicitWaitTimeout <- TimeSpan.FromSeconds 10.0
        new EdgeDriver(service, options)

let private parsePageText (driver: IWebDriver) =
    driver.FindElement(By.TagName "body").Text

[<Struct>]
type Webpage =
    { Content: string
      Url: string
      Title: string }

let fetchWebpageAsync (driver: IWebDriver) (url: string) : Async<Webpage> =
    async {
        do! driver.Navigate().GoToUrlAsync url |> Async.AwaitTask

        return
            { Content = parsePageText driver
              Url = url
              Title = driver.Title }
    }
