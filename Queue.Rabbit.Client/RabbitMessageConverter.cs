using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Rabbit.Client.Interfaces;
using Queue.Rabbit.Core;
using Queue.Rabbit.Core.Interfaces;
using RabbitMQ.Client.Events;
using ContentDispositionHeaderValue = System.Net.Http.Headers.ContentDispositionHeaderValue;
using ContentRangeHeaderValue = System.Net.Http.Headers.ContentRangeHeaderValue;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Queue.Rabbit.Client;

/// <inheritdoc />
public class RabbitMessageConverter : IRabbitMessageConverter
{
    public const int DefaultStatusCode = 200;
    private readonly ILogger<RabbitMessageConverter> _log;
    private readonly IBasicDeliverEventArgsValidator _validator;

    /// <inheritdoc />
    public RabbitMessageConverter(ILogger<RabbitMessageConverter> log, IBasicDeliverEventArgsValidator validator)
    {
        _log = log;
        _validator = validator;
    }

    /// <summary>
    /// Прочитать заголовок
    /// </summary>
    /// <param name="header">Заголовок (ключ/значение)</param>
    /// <returns>Строковое значение</returns>
    public string ReadHeader(KeyValuePair<string, object> header)
    {
        if (header.Value is byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        try
        {
            return header.Value.ToString();
        }
        catch (Exception e)
        {
            _log.LogError(e, $"Not read value {header.Key}");

            return null;
        }
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> TryParse(BasicDeliverEventArgs args)
    {
        _validator.Validate(args);

        try
        {
            var props = args.BasicProperties;
            var content = new ReadOnlyMemoryContent(args.Body);
            var contentHeaders = content.Headers;
            var contentTypeHeader = props.Headers.ExtractHeader(HeaderNames.ContentType);
            var expiresHeader = props.Headers.ExtractHeader(HeaderNames.Expires);
            var contentDispositionHeader = props.Headers.ExtractHeader(HeaderNames.ContentDisposition);
            var contentMd5Header = props.Headers.GetOrDefault(HeaderNames.ContentMD5) as byte[];
            props.Headers.Remove(HeaderNames.ContentMD5);
            var contentRangeHeader = props.Headers.ExtractHeader(HeaderNames.Range);
            var contentLastModifiedHeader = props.Headers.ExtractHeader(HeaderNames.LastModified);
            var contentLocationHeader = props.Headers.ExtractHeader(HeaderNames.ContentLocation);

            if (!MediaTypeHeaderValue.TryParse(contentTypeHeader, out var mediaType))
            {
                _log.LogError($"Не удалось распознать заголовок {HeaderNames.ContentType}:{contentTypeHeader}");
            }
            contentHeaders.ContentType = mediaType;

            contentHeaders.Expires = DateTimeOffset.TryParse(expiresHeader, out var expires) ? expires : (DateTimeOffset?)null;
            contentHeaders.LastModified = DateTimeOffset.TryParse(contentLastModifiedHeader, out var lastModified) ? lastModified : (DateTimeOffset?)null;
            contentHeaders.ContentRange = ContentRangeHeaderValue.TryParse(contentRangeHeader, out var contentRange) ? contentRange : null;
            contentHeaders.ContentDisposition = ContentDispositionHeaderValue.TryParse(contentDispositionHeader, out var contentDisposition) ? contentDisposition : null;
            if (Uri.TryCreate(contentLocationHeader, UriKind.RelativeOrAbsolute, out var contentLocation))
            {
                contentHeaders.ContentLocation = contentLocation;
            }

            if (contentMd5Header != null)
            {
                contentHeaders.ContentMD5 = contentMd5Header;
            }

            var statusCodeHeader = props.Headers.GetOrDefaultString(RabbitConsts.Status);

            if (!int.TryParse(statusCodeHeader, out var statusCode))
            {
                statusCode = DefaultStatusCode;
            }

            var response = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)statusCode,
                Content = content
            };
            response.Headers.AddReply(props.CorrelationId);

            foreach (var header in props.Headers)
            {
                if (header.Key == RabbitConsts.Status || header.Key == HeaderNames.ContentLength)
                {
                    continue;
                }

                response.Headers.Add(header.Key, ReadHeader(header));
            }

            return Task.FromResult(response);
        }
        catch (Exception e)
        {
            _log.LogError(e, "Ошибка при парсинге ответа");

            return null;
        }
    }
}