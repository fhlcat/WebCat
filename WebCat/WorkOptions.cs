using WebCat.Web.Browser;

namespace WebCat;

public readonly record struct WorkOptions(
    int Interval,
    string Endpoint,
    string ApiKey,
    string Model,
    bool Headless,
    Utils.BrowserType BrowserType
)
{
    public readonly string ApiKey = ApiKey;
    public readonly string Endpoint = Endpoint;
    public readonly int Interval = Interval;
    public readonly string Model = Model;
    public readonly bool Headless = Headless;
    public readonly Utils.BrowserType BrowserType = BrowserType;
}