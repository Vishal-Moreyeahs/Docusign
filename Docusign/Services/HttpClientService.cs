using Docusign.Interface;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Docusign.Services
{
    public class HttpClientService : IHttpClientService
    {
        public async Task<HttpResponseMessage> Get(string requestUrl,string headers, string contentType, string acceptType)
        {
            //Create http Request
            using (HttpClient client = new HttpClient())
            {
                // Set Zoho Books API key in the Authorization header
                // Set the base address of the API
                client.BaseAddress = new Uri(requestUrl);

                // Set the authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", headers);

                // Set the content type header
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                { 
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                    else
                    {
                        throw new ApplicationException("Data Not Found");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error : ",ex);
                }

                
            }
        }

    }
}
