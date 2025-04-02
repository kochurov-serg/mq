using System;

namespace Queue.Core;

public class QueueNameExtensions
{
    public static string GetQueue(Uri uri, Uri relativeUri)
    {
        var fullUri = new Uri(uri, relativeUri);

        return GetQueue(fullUri);
    }

    public static string GetQueue(Uri uri) =>string.Concat(uri.Host, uri.PathAndQuery, uri.Fragment);
}