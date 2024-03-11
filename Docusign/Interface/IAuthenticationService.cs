using static DocuSign.Admin.Client.Auth.OAuth;

namespace Docusign.Interface
{
    public interface IAuthenticationService
    {
        OAuthToken AuthenticateWithJwt(string api, string clientId, string impersonatedUserId, string authServer, byte[] privateKeyBytes);
    }
}
