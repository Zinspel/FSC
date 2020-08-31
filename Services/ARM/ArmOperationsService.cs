using AuditQualification.Models;
using AuditQualification.Services.GraphOperations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AuditQualification.Services.Arm
{
    public class ArmApiOperationService : IArmOperations
    {
        private readonly HttpClient httpClient;
        private readonly WebOptions webOptions;

        public ArmApiOperationService(HttpClient httpClient, IOptions<WebOptions> webOptionValue)
        {
            this.httpClient = httpClient;
            webOptions = webOptionValue.Value;
        }

        /// <summary>
        /// Enumerates the list of Tenant IDs accessible for a user. Gets a token for the user
        /// and calls the ARM API.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> EnumerateTenantsIdsAccessibleByUser(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var httpResult = await httpClient.GetAsync(ArmListTenantUrl);
            string json = await httpResult.Content.ReadAsStringAsync();
            ArmResult armTenants = JsonConvert.DeserializeObject<ArmResult>(json);
            return armTenants.value.Select(t => t.tenantId);
        }
        public async Task<string> GetAccessToken()
        {
            var list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("grant_type", "password"));
            list.Add(new KeyValuePair<string, string>("resource", webOptions.Resource));
            list.Add(new KeyValuePair<string, string>("client_id", webOptions.ClientId));
            list.Add(new KeyValuePair<string, string>("client_secret", webOptions.ClientSecret));
            list.Add(new KeyValuePair<string, string>("username", webOptions.EmailAddress));
            list.Add(new KeyValuePair<string, string>("password", webOptions.Password));

            HttpContent content = new FormUrlEncodedContent(list);

            //  var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentType.CharSet = "UTF-8";
            var httpResult = await httpClient.PostAsync("https://login.microsoftonline.com/b4f36542-1635-471c-bbee-4b6693d7e26a/oauth2/token", content);
            var answer = await httpResult.Content.ReadAsStringAsync();
            var accessToken = JsonConvert.DeserializeObject<AccessTokenResult>(answer);
            return accessToken.access_token;
        }

        public async Task<string> GetAccountsAsync(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var httpResult = await httpClient.GetAsync("https://sandboxhuiskes.api.crm4.dynamics.com/api/data/v9.1/organizations");
            string json = await httpResult.Content.ReadAsStringAsync();
            return json;
        }

        public async Task<CompanyDetailsModel> GetCompanyDetails(string accessToken, string companyId)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var httpResult = await httpClient.GetAsync("https://sandboxhuiskes.api.crm4.dynamics.com/api/data/v9.1/organizations");
            string json = await httpResult.Content.ReadAsStringAsync();

            return new CompanyDetailsModel();
        }

        // Use Azure Resource manager to get the list of a tenant accessible by a user
        // https://docs.microsoft.com/en-us/rest/api/resources/tenants/list
        public static string ArmResource { get; } = "https://management.core.windows.net/";

        protected string ArmListTenantUrl { get; } = "https://management.azure.com/tenants?api-version=2016-06-01";
    }

    public class AccessTokenResult
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string expires_on { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}