using AuditQualification.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuditQualification.Services.Arm
{
    public interface IArmOperations
    {
        Task<IEnumerable<string>> EnumerateTenantsIdsAccessibleByUser(string accessToken);
        Task<string> GetAccountsAsync(string accessToken);
        Task<string> GetAccessToken();
        Task<CompanyDetailsModel> GetCompanyDetails(string accessToken, string companyId);
    }
}