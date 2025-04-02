using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Queue.Server.Abstractions;

public class ResponseConverter: IResponseConverter
{
    public async Task<byte[]> Convert(HttpResponse response)
    {
        if (response.Body.CanSeek)
            response.Body.Position = 0;

        var message = new HttpResponseMessage
        {
            StatusCode = (HttpStatusCode)response.StatusCode,
            Content = new StreamContent(response.Body),
        };

        message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(response.ContentType);
        message.Content.Headers.ContentLength = response.Body.Length;

        foreach (var header in response.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable());
        }

        var content = new HttpMessageContent(message);
        var bytes = await content.ReadAsByteArrayAsync();

        return bytes;
    }

}

public interface IResponseConverter
{
    Task<byte[]> Convert(HttpResponse response);
}