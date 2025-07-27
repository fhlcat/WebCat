using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WebCat.Browser;

public static class Utils
{
    public static ChromeDriver Init()
    {
        var options = new ChromeOptions();
        //todo simplify this
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-web-security");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--start-maximized");
        options.AddArgument("--user-data-dir=/dev/null");
        // options.add_experimental_option('excludeSwitches', ['enable-automation', 'useAutomationExtension'])

        return new ChromeDriver(options);
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