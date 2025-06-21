using System.Collections.Concurrent;

namespace Commander.Controllers;

static class Cancellations
{
    public static CancellationToken ChangePathCancellation(string id)
    {
        lock (locker)
        {
            if (changePathCancellations.TryGetValue(id, out CancellationTokenSource? value))
                value.Cancel();
            var cts = new CancellationTokenSource();
            changePathCancellations[id] = cts;
            return cts.Token;
        }
    }

    static readonly Dictionary<string, CancellationTokenSource> changePathCancellations = [];

    static readonly Lock locker = new();
}