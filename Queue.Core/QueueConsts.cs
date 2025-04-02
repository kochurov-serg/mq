using System;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Queue.Core;

/// <summary>
/// Communication const client to server
/// </summary>
public static class QueueConsts
{
    /// <summary>
    /// Default content type application/json
    /// </summary>
    public const string DefaultContentType = "application/json";

    public static readonly HashSet<string> ContentHeaders =
    [
        HeaderNames.ContentEncoding.ToLowerInvariant(),
        HeaderNames.ContentLength.ToLowerInvariant(),
        HeaderNames.ContentType.ToLowerInvariant(),
        HeaderNames.ContentMD5.ToLowerInvariant(),
        HeaderNames.ContentLanguage.ToLowerInvariant(),
        HeaderNames.ContentLocation.ToLowerInvariant(),
        HeaderNames.ContentDisposition.ToLowerInvariant()
    ];
}