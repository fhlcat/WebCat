using System.Threading.Channels;

namespace WebCat;

public static class AsyncEnumerableExtension
{
    public static async IAsyncEnumerable<T1> MapiAsync<T0, T1>(
        this IAsyncEnumerable<T0> source,
        Func<T0, int, Task<T1>> mapFunc
    )
    {
        var index = 0;
        await foreach (var item in source)
        {
            yield return await mapFunc(item, index++);
        }
    }

    public static async IAsyncEnumerable<T1> MapiAsync<T0, T1>(
        this IEnumerable<T0> source,
        Func<T0, int, Task<T1>> mapFunc
    )
    {
        var index = 0;
        foreach (var item in source)
        {
            yield return await mapFunc(item, index++);
        }
    }

    private static async Task WriteAllEnumerableAsync<T>(this Channel<T> channel, IAsyncEnumerable<T> items)
    {
        await foreach (var item in items)
        {
            await channel.Writer.WriteAsync(item);
        }

        channel.Writer.Complete();
    }

    public static IAsyncEnumerable<T> LoadAsync<T>(this IAsyncEnumerable<T> source)
    {
        var channel = Channel.CreateUnbounded<T>();
        _ = channel.WriteAllEnumerableAsync(source);

        return channel.Reader.ReadAllAsync();
    }
}