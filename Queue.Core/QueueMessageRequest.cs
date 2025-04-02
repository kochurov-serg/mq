using System;
using System.Collections.Generic;
using System.IO;

namespace Queue.Core;

/// <summary>
/// message (request/response)
/// </summary>
public class QueueMessageRequest
{
    /// <summary>
    /// Uri address
    /// </summary>
    public Uri Uri { get; set; }
    /// <summary>
    /// Body
    /// </summary>
    public Stream Body { get; set; }
    /// <summary>
    /// Headers
    /// </summary>
    public IDictionary<string, IEnumerable<string>> Headers { get; set; }
    /// <summary>
    /// Properties
    /// </summary>
    public IDictionary<string, IEnumerable<string>> Properties { get; set; }
}