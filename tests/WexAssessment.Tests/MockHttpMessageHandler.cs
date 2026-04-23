using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WexAssessment.Tests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseContent;

    public MockHttpMessageHandler(string responseContent)
    {
        _responseContent = responseContent;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(_responseContent)
        });
    }
}