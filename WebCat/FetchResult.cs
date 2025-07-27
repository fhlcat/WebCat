using WebCat.Web;

namespace WebCat;

public readonly record struct FetchResult(SearchResult SearchResult, Webpage Webpage)
{
    public readonly SearchResult SearchResult = SearchResult;
    public readonly Webpage Webpage = Webpage;
};