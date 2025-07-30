namespace WebCat.Process.Struct;

public readonly record struct ProcessRequest(string Article, string Question)
{
    public readonly string Article = Article;
    public readonly string Question = Question;
}