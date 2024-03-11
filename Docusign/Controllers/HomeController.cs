using Docusign.Helpers;
using Docusign.Interface;
using Docusign.Models;
using Docusign.Services;
using DocuSign.eSign.Client;
using static DocuSign.eSign.Client.Auth.OAuth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DocuSign.eSign.Model;
using System.Xml.Linq;
using User = Docusign.Models.User;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using DocuSign.eSign.Api;
using System.Security.Cryptography.Xml;
using Signature = Docusign.Models.Signature;
using iText.Kernel.Pdf;
using static DocuSign.Admin.Client.Auth.OAuth.UserInfo;
using Account = DocuSign.eSign.Client.Auth.OAuth.UserInfo.Account;

namespace Docusign.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDocumentService _documentService;
        //private readonly IAuthenticationService _authenticationService;

        public HomeController(ILogger<HomeController> logger
            //, IAuthenticationService authenticationService
            , IDocumentService documentService
            )
        {
            //_authenticationService = authenticationService;
            _logger = logger;
            _documentService = documentService;
        }

        [HttpGet]
        public IActionResult FirstPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(Models.User user)
        {
            // Save user data to database or session
            TempData["FirstName"] = user.FirstName;
            TempData["LastName"] = user.LastName;
            var firstName = TempData["FirstName"].ToString();
            var lastName = TempData["LastName"].ToString();

            //Authentication Process
            var clientId = "2859ce0a-3c71-4796-a508-3621939ac2fb";
            var impersonateId = "84213e37-5ba9-458d-8b30-c6d85118a794";
            var authServer = "account-d.docusign.com";
            var privateKeyFile = Path.Combine(Directory.GetCurrentDirectory() + "\\" + "private.key");
            var accessToken = AuthenticationService.AuthenticateWithJwt("ESignature", clientId, impersonateId,
                                                        authServer, DsHelper.ReadFileContent(privateKeyFile));

            //AAuthentication Process

            //Authorization Process
            var docuSignClient = new DocuSignClient();
            docuSignClient.SetOAuthBasePath(authServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClient.GetUserInfo(accessToken.access_token);
            Account acct = userInfo.Accounts.FirstOrDefault();
            //Authorization Process

            //Temp
            var htmlContent = "";
            using (var reader = new StreamReader(@"Views/Home/" + "Pdf.cshtml"))
            {
                htmlContent = await reader.ReadToEndAsync();
            }
            var userData = new User
            {
                FirstName = firstName,
                LastName = lastName
            };

            var html = _documentService.PopulateHtmlWithDynamicValues(htmlContent, userData);
            var pdfBytes = _documentService.ConvertHtmlToPdf(html);
            //Temp
            var envelopeData = SigningViaEmail.SendEnvelopeViaEmail(pdfBytes, "vishal.pawar5898@gmail.com", firstName, "vishal.pawar5898@gmail.com", lastName, accessToken.access_token, acct.BaseUri + "/restapi", acct.AccountId, "", "sent");

            TempData["Url"] = envelopeData.SignedUrl;
            TempData["EnvelopeId"] = envelopeData.EnvelopeId;

            // Call your DocuSign service to complete registraation with the signature
            // Assuming you have a service to handle DocuSign integration

            // After successful registration, you may redirect to a thank you page or perform any other actions
            //return RedirectToAction("ThankYou");
            return RedirectToAction("SecondPage");
        }

        [HttpGet]
        public IActionResult SecondPage()
        {
            var url = TempData["Url"].ToString();
            var sign = new Signature { SignatureData=url };
            return View(sign);
        }

        [HttpPost]
        public async Task<IActionResult> SignAsync(Signature signature)
        {
            // Retrieve user data from TempData (You may need to change this based on your actual data storage)
            var firstName = TempData["FirstName"].ToString();
            var lastName = TempData["LastName"].ToString();

            //Authentication Process
            var clientId = "2859ce0a-3c71-4796-a508-3621939ac2fb";
            var impersonateId = "84213e37-5ba9-458d-8b30-c6d85118a794";
            var authServer = "account-d.docusign.com";
            var privateKeyFile  = Path.Combine(Directory.GetCurrentDirectory() +"\\"+ "private.key");
            var accessToken = AuthenticationService.AuthenticateWithJwt("ESignature", clientId, impersonateId,
                                                        authServer, DsHelper.ReadFileContent(privateKeyFile));

            //AAuthentication Process

            //Authorization Process
            var docuSignClient = new DocuSignClient();
            docuSignClient.SetOAuthBasePath(authServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClient.GetUserInfo(accessToken.access_token);
            Account acct = userInfo.Accounts.FirstOrDefault();
            //Authorization Process

            //Temp
            var htmlContent = "";
            using (var reader = new StreamReader(@"Views/Home/" + "Pdf.cshtml"))
            {
                htmlContent = await reader.ReadToEndAsync();
            }
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName
            };

            var html = _documentService.PopulateHtmlWithDynamicValues(htmlContent,user);
            var pdfBytes = _documentService.ConvertHtmlToPdf(html);
            //Temp
            string envelopeId = SigningViaEmail.SendEnvelopeViaEmail(pdfBytes,"vishal.pawar5898@gmail.com", firstName, "vishal.pawar5898@gmail.com", lastName, accessToken.access_token, acct.BaseUri + "/restapi", acct.AccountId, "", "sent");

           

            // Call your DocuSign service to complete registraation with the signature
            // Assuming you have a service to handle DocuSign integration

            // After successful registration, you may redirect to a thank you page or perform any other actions
            return RedirectToAction("Download");
        }

        public IActionResult Download()
        {
            var envelopeId = TempData["EnvelopeId"].ToString();
            //Authentication Process
            var clientId = "2859ce0a-3c71-4796-a508-3621939ac2fb";
            var impersonateId = "84213e37-5ba9-458d-8b30-c6d85118a794";
            var authServer = "account-d.docusign.com";
            var privateKeyFile = Path.Combine(Directory.GetCurrentDirectory() + "\\" + "private.key");
            var accessToken = AuthenticationService.AuthenticateWithJwt("ESignature", clientId, impersonateId,
                                                        authServer, DsHelper.ReadFileContent(privateKeyFile));

            //AAuthentication Process

            //Authorization Process
            var docuSignClient = new DocuSignClient();
            docuSignClient.SetOAuthBasePath(authServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClient.GetUserInfo(accessToken.access_token);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo.Account acct = userInfo.Accounts.FirstOrDefault();
            var docSign = new DocuSignClient(acct.BaseUri);
            EnvelopesApi envelopesApi = new EnvelopesApi(docSign);
            //docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken.access_token);

            var results = envelopesApi.ListDocuments("accountId", envelopeId);

            var combinedPdf = new MemoryStream();
            return File(combinedPdf.ToArray(), "application/pdf", $"Envelope_{envelopeId}.pdf");

        }

        public IActionResult ThankYou()
        {
            return View();
        }

        public IActionResult Index()
        {   
            return View("FirstPage");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
