using WebCat.Fetch.Struct;

namespace WebCat.Struct;

public readonly record struct FetchResult(SearchEngineResult SearchEngineResult, Webpage Webpage)
{
    public readonly SearchEngineResult SearchEngineResult = SearchEngineResult;
    public readonly Webpage Webpage = Webpage;
};