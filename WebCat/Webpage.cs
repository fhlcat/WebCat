namespace WebCat;

public readonly record struct Webpage(string Title, string Content)
{
    public readonly string Title = Title;
    public readonly string Content = Content;
}