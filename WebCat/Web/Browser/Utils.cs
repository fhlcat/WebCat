using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace WebCat.Web.Browser;

public static class Utils
{
    private static readonly string[] ArgumentsToExclude = ["enable-automation", "useAutomationExtension"];

    public enum BrowserType
    {
        Edge,
        Chrome
    }

    private static List<string> GetWebDriverArguments(bool headless)
    {
        var arguments = new List<string>
        {
            "--disable-blink-features=AutomationControlled",
            "--disable-extensions",
            // ReSharper disable once StringLiteralTypo
            "--disable-infobars",
            "--disable-notifications",
            "--disable-popup-blocking",
            "--disable-web-security",
            "--ignore-certificate-errors",
            "--no-sandbox",
            "--start-maximized",
            "--disable-background-networking",
            "--disable-default-apps",
            // ReSharper disable once StringLiteralTypo
            "--enable-unsafe-swiftshader"
        };
        if (headless)
        {
            arguments.AddRange("--headless", "--disable-gpu");
        }

        return arguments;
    }

    private static EdgeDriver InitializeEdgeDriver(bool headless)
    {
        Environment.SetEnvironmentVariable("SE_DRIVER_MIRROR_URL", "https://msedgedriver.microsoft.com");

        var service = EdgeDriverService.CreateDefaultService();
#if !DEBUG
                     service.HideCommandPromptWindow = true;
#endif

        var options = new EdgeOptions();
        var arguments = GetWebDriverArguments(headless);

        options.AddArguments(arguments);
        options.AddExcludedArguments(ArgumentsToExclude);

        return new EdgeDriver(service, options);
    }

    public static ChromeDriver InitializeChromeDriver(bool headless)
    {
        var service = ChromeDriverService.CreateDefaultService();
#if !DEBUG
                     service.HideCommandPromptWindow = true;
#endif

        var options = new ChromeOptions();
        var arguments = GetWebDriverArguments(headless);

        options.AddArguments(arguments);
        options.AddExcludedArguments(ArgumentsToExclude);

        return new ChromeDriver(service, options);
    }

    public static IWebDriver Init(BrowserType browserType, bool headless)
    {
        return browserType switch
        {
            BrowserType.Edge => InitializeEdgeDriver(headless),
            BrowserType.Chrome => InitializeChromeDriver(headless),
            _ => throw new ArgumentOutOfRangeException(nameof(browserType))
        };
    }

    public static string? GetInnerText(this IWebElement element) => element.GetAttribute("innerText");

    public static string? GetBodyInnerText(this IWebDriver driver) =>
        driver.FindElement(By.TagName("body")).GetInnerText();

    public static async Task<Webpage> FetchWebpageAsync(IWebDriver driver, string url)
    {
        await driver.Navigate().GoToUrlAsync(url);
        return new Webpage(driver.Title, driver.GetBodyInnerText()!);
    }
}