namespace WebCat.Ai;

public readonly record struct AiRequest(string Article, string Question)
{
    public readonly string Article = Article;
    public readonly string Question = Question;
}