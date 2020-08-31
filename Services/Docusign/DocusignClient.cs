using System;
using System.Linq;
using System.Text;

using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Security;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace AuditQualification.Service
{
    public class DocusignClient : IDocusignClient
    {
        private IConfiguration _configuration;
        private string AccessToken;
        private DateTime expiresIn;
        protected Configuration Config { get; private set; }
        private ApiClient _ApiClient;
        public Account Account { get; private set; }

        public DocusignClient(IConfiguration configuration)
        {
            _configuration = configuration;
            CheckToken();
            Config = new Configuration(_ApiClient.Configuration.BasePath);
            Config.AddDefaultHeader("Authorization", "Bearer " + AccessToken);
        }

        public string CreateEnvelope(EnvelopeDefinition envelope)
        {
            var envelopesApi = new EnvelopesApi(_ApiClient);
            var results = envelopesApi.CreateEnvelope(Account.AccountId, envelope);

            return results.EnvelopeId;
        }



        public string CreateRecipientView(string envelopeId, RecipientViewRequest viewRequest)
        {
            var envelopesApi = new EnvelopesApi(_ApiClient);
            var recipientView = envelopesApi.CreateRecipientView(Account.AccountId, envelopeId, viewRequest);
            return recipientView.Url;
        }

        private void CheckToken()
        {
            if (AccessToken == null || DateTime.UtcNow > expiresIn)
            {
                UpdateToken();
            }
        }

        private void UpdateToken()
        {
            _ApiClient = new ApiClient();
            var clientid = _configuration["docusign:clientid"];
            var authorizationServer = _configuration["Docusign:AuthorizationServer"];
            var privatekey = _configuration["Docusign:PrivateKey"];
            var authToken = _ApiClient.RequestJWTApplicationToken(clientid,
                                                        authorizationServer,
                                                        Encoding.UTF8.GetBytes(privatekey),
                                                        1);

            AccessToken = authToken.access_token;

            _ApiClient = new ApiClient("https://demo.docusign.net/restapi");

            expiresIn = DateTime.UtcNow.AddSeconds(authToken.expires_in.Value);
        }


    }
}
