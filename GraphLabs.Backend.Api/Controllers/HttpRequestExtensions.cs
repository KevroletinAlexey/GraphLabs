using System.Text;

namespace WebApplication2.Controllers;

internal static class HttpRequestExtensions
{
    public static async Task<string> GetBodyAsString(this HttpRequest request)
    {
        using (var reader = new StreamReader(request.Body, Encoding.UTF8))
        {
            return await reader.ReadToEndAsync();
        }
    }
}