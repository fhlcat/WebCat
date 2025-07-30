namespace WebCat.Fetch.Struct;

public readonly record struct SearchEngineResult(string Title, string Url)
{
    public readonly string Title = Title;
    public readonly string Url = Url;
}