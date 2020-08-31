using System;
using System.Linq;
using System.Text;

using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using Microsoft.Extensions.Configuration;
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



        internal string CreateRecipientView(string envelopeId, RecipientViewRequest viewRequest)
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
            var authToken = new OAuth.OAuthToken();

            authToken = _ApiClient.RequestJWTUserToken(_configuration["DocusignClientId"],
                                                        _configuration["DocusignImpersonatedUserGuid"], 
                                                        _configuration["DocusignAuthorizationServer"],
                                                        Encoding.UTF8.GetBytes(_configuration["DocusignPrivateKey"]),
                                                        1);

            AccessToken = authToken.access_token;

            if (Account == null)
                Account = GetAccountInfo(authToken);

            _ApiClient = new ApiClient(Account.BaseUri + "/restapi");

            expiresIn = DateTime.UtcNow.AddSeconds(authToken.expires_in.Value);
        }

        private Account GetAccountInfo(OAuth.OAuthToken authToken)
        {
            _ApiClient.SetOAuthBasePath(_configuration["DocusignAuthorizationServer"]);
            var userInfo = _ApiClient.GetUserInfo(authToken.access_token);
            Account acct = null;

            var accounts = userInfo.Accounts;
            var targetAccountId = _configuration["DocusignTargetAccountId"];
            if (!string.IsNullOrEmpty(targetAccountId) && !targetAccountId.Equals("FALSE"))
            {
                acct = accounts.FirstOrDefault(a => a.AccountId == targetAccountId);

                if (acct == null)
                {
                    throw new Exception("The user does not have access to account " + targetAccountId);
                }
            }
            else
            {
                acct = accounts.FirstOrDefault(a => a.IsDefault == "true");
            }

            return acct;
        }


    }
}
