namespace WebCat.Web;

public readonly record struct SearchResult(string Title, string Url)
{
    public readonly string Title = Title;
    public readonly string Url = Url;
}