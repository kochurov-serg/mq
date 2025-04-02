
namespace Queue.Core;

/// <summary>
/// Заголовки
/// </summary>
public static class QueueHeaders
{
    /// <summary>
    /// Относительный адрес запроса
    /// </summary>
    public const string Uri = "uri";
    /// <summary>
    /// Method
    /// </summary>
    public const string Method = "method";
    /// <summary>
    /// Response id. Set if need response service
    /// </summary>
    public const string ReplyId = "reply-Id";
    /// <summary>
    /// Priority
    /// </summary>
    public const string Priority = "priority";
    /// <summary>
    /// Status
    /// </summary>
    public const string Status = "status";

    /// <summary>
    /// Send response to queue
    /// </summary>
    public const string ReplyTo = "reply-to";
}