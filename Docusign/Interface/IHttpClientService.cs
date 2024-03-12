namespace Docusign.Interface
{
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> Get(string requestUrl, string headers, string contentType, string acceptType);
    }
}
