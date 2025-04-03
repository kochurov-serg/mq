using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Queue.Core;
using Queue.Server.Abstractions.Interfaces;

namespace Queue.Server.Abstractions;

public class HttpContextCreator : IHttpContextCreator
{
    private readonly ILogger<HttpContextCreator> _log;
    private readonly IQueueRequestConverter _transform;

    public HttpContextCreator(ILogger<HttpContextCreator> log, IQueueRequestConverter transform)
    {
        _log = log;
        _transform = transform;
    }

    public async Task<HttpContext> Create(byte[] bytes, IFeatureCollection features, bool needResponseStream)
    {
        var request = await _transform.Convert(bytes);

        if (request == null)
            return null;

        request.Headers.TryGetValue(HeaderNames.Accept, out var acceptValues);
        var accept = acceptValues.ToString();
        var response = new QueueResponse
        {
            ContentType =  string.IsNullOrWhiteSpace(accept) ? QueueConsts.DefaultContentType : accept,
            StatusCode = StatusCodes.Status200OK,
            Body = needResponseStream ? Stream.Null : new MemoryStream()
        };

        _log.LogTrace("Create queue context");
        var context = new QueueContext(features, response, request);
        _log.LogTrace("Create context created {uri}", request.GetDisplayUrl());

        return context;
    }
}