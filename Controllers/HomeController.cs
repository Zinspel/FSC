using AuditQualification.Infrastructure;
using AuditQualification.Models;
using AuditQualification.Services.Arm;
using AuditQualification.Services.GraphOperations;
using Docusign.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AuditQualification.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        readonly ITokenAcquisition tokenAcquisition;
        private readonly IGraphApiOperations graphApiOperations;
        private readonly IArmOperations armOperations;
        private IDocusignService _docusignService;

        public HomeController(IDocusignService docusignService,
                                ITokenAcquisition tokenAcquisition,
                                IGraphApiOperations graphApiOperations,
                                IArmOperations armOperations)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.graphApiOperations = graphApiOperations;
            this.armOperations = armOperations;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel();
            var accessToken = await armOperations.GetAccessToken();

            model.Jsonstring = await armOperations.GetAccountsAsync(accessToken);
            model.Companies.Add(new CompanyModel { Date = DateTime.Now, ID = "1", Name = "Chopper" });
            return View(model);
        }

        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]
        public async Task<IActionResult> Profile()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] { Constants.ScopeUserRead });

            var me = await graphApiOperations.GetUserInformation(accessToken);
            var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

            ViewData["Me"] = me;
            ViewData["Photo"] = photo;

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult Sign(string id)
        {
            byte[] buffer = System.IO.File.ReadAllBytes("Docs/R.11_Justification_for_Remote_Audit.pdf");
            var doc = Convert.ToBase64String(buffer);
            var envelopeId = _docusignService.CreateEnvelope(User.Identity.Name,User.Identity.Name,doc);
            var redirectUrl = _docusignService.CreateRecipientView(User.Identity.Name, User.Identity.Name, envelopeId);
            return Redirect(redirectUrl);
        }


        public async Task<IActionResult> DetailsAsync(string id)
        {

            var accessToken = await armOperations.GetAccessToken();
            var company = await armOperations.GetCompanyDetails(accessToken,id);
            var model = CreateCompanyModel(company);
            return View(model);
        }

        private CompanyDetailsModel CreateCompanyModel(object company)
        {
            var model = new CompanyDetailsModel()
            {

                Location = new Location { Country = "NL" },
                Risks = new Risks
                {
                    CorruptionIndexScore = "54",
                    CorruptionIndexScore_Flagged = true,
                    COVID_19_Flagged = true,
                    COVID_19_riskscore = "19%",
                    DeforestationRisk = "yes",
                    DeforestationRisk_Flagged = true
                },
                Name = "Test"
            };
            return model;
        }
    }
}