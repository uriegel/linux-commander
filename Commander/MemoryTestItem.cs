using System.Collections.Immutable;

record MemoryTestItem : IDisposable
{
    public string Display { get; }
    public MemoryTestItem(string display)
    {
        Display = display;
        items = items.Add(this, Display);
    }

    public static void Snapshot() => Console.WriteLine($"Anzahl aller TestItems: {items.Count}");

    public static ImmutableDictionary<MemoryTestItem, string> items = ImmutableDictionary<MemoryTestItem, string>.Empty;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            items = items.Remove(this);
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            disposedValue = true;
        }
    }

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~MemoryTestItem()
    {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    bool disposedValue;
}
