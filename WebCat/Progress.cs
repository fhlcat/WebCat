namespace WebCat;

public readonly record struct Progress<T>(T Current, int CurrentCount, int Total)
{
    public readonly T Current = Current;
    public readonly int CurrentCount = CurrentCount;
    public readonly int Total = Total;
}