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
using Newtonsoft.Json;
using DocuSign.Admin.Client;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using iText.Kernel.Pdf.Action;

namespace Docusign.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDocumentService _documentService;
        private readonly IHttpClientService _client;

        public HomeController(ILogger<HomeController> logger
            , IHttpClientService client
            , IDocumentService documentService
            )
        {
            _client = client;
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
            var firstName = TempData.Peek("FirstName").ToString();
            var lastName = TempData.Peek("LastName").ToString();

            //Authentication Process
            var clientId = "e03c8ec1-c28c-4029-98d6-a534d7d062cc";
            var impersonateId = "84213e37-5ba9-458d-8b30-c6d85118a794";
            var authServer = "account-d.docusign.com";
            var privateKeyFile = Path.Combine(Directory.GetCurrentDirectory() + "\\" + "private.key");
            var accessToken = AuthenticationService.AuthenticateWithJwt("ESignature", clientId, impersonateId,
                                                        authServer, DsHelper.ReadFileContent(privateKeyFile));
            TempData["AccessToken"] = accessToken.access_token;
            //AAuthentication Process

            //Authorization Process
            var docuSignClient = new DocuSign.eSign.Client.DocuSignClient();
            docuSignClient.SetOAuthBasePath(authServer);
            DocuSign.eSign.Client.Auth.OAuth.UserInfo userInfo = docuSignClient.GetUserInfo(accessToken.access_token);
            Account acct = userInfo.Accounts.FirstOrDefault();
            //Authorization Process
            TempData["BaseUri"] = acct.BaseUri;
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
            ViewBag.Status = true;
            TempData["Url"] = envelopeData.SignedUrl;
            TempData["EnvelopeId"] = envelopeData.EnvelopeId;
            //TempData["EnvelopData"] = envelopeData.EnvelopesApiData;
            // Call your DocuSign service to complete registraation with the signature
            // Assuming you have a service to handle DocuSign integration

            // After successful registration, you may redirect to a thank you page or perform any other actions
            //return RedirectToAction("ThankYou");
            return RedirectToAction("SecondPage");
        }

        [HttpGet]
        public IActionResult SecondPage()
        {
            ViewBag.Status = true;
            var url = "";
            try
            {
                url = TempData.Peek("Url").ToString();
            }
            catch (Exception ex)
            {
                url = "";
            }
            var sign = new Signature { SignatureData=url };
            return View(sign);
        }

        public async Task<IActionResult> Check()
        {
            var envelopeId = TempData.Peek("EnvelopeId").ToString();
            var accountId = "f9c7a179-50f9-4c0d-84e1-3bf060b19999";
            var baseUri = TempData.Peek("BaseUri").ToString();
            var accessToken = TempData.Peek("AccessToken").ToString();

            var documentRequest = $"https://demo.docusign.net/restapi/v2.1/accounts/{accountId}/envelopes/{envelopeId}/documents";
            var documents = await _client.Get(documentRequest, accessToken, "application/json", "");
            string responseData = await documents.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<EnvelopeDocumentsResult>(responseData);
            var doc = response.EnvelopeDocuments.FirstOrDefault();
            var pdfLink = $"https://demo.docusign.net/restapi/v2.1/accounts/{accountId}{doc.Uri}";
            var pdfResponseData = await _client.Get(pdfLink,accessToken, "application/pdf", "");
            //var responsePdfData = await pdfResponseData.Content.ReadAsStringAsync();
            Stream pdfStream = await pdfResponseData.Content.ReadAsStreamAsync();
            // Save the stream to a file
            //string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Pdfs" ,"Envelope1.pdf"); // Path where you want to save the PDF file
            //using (FileStream fileStream = System.IO.File.Create(filePath))
            //{
            //    await pdfStream.CopyToAsync(fileStream);
            //}
            // Set content disposition header
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "envelope-document.pdf",
                Inline = false // false = prompt the user for downloading; true = inline display
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(pdfStream, "application/pdf");
        }

        public async Task<bool> InitialCheck()
        {
            var envelopeId = TempData.Peek("EnvelopeId").ToString();
            var accountId = "f9c7a179-50f9-4c0d-84e1-3bf060b19999";
            var baseUri = TempData.Peek("BaseUri").ToString();
            var accessToken = TempData.Peek("AccessToken").ToString();

            var requestUrl = $"https://demo.docusign.net/restapi/v2.1/accounts/{accountId}/envelopes/{envelopeId}";
            var data = await _client.Get(requestUrl, accessToken, "application/json", "");
            string responseData = await data.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<Envelope>(responseData);

            if (response.Status == "sent")
            {
                return false;
            }
            else 
            {
                return true;
            }

        }

        public async Task<string> ShowImageAsync()
        {
            var envelopeId = TempData.Peek("EnvelopeId").ToString();
            var accountId = "f9c7a179-50f9-4c0d-84e1-3bf060b19999";
            var baseUri = TempData.Peek("BaseUri").ToString();
            var accessToken = TempData.Peek("AccessToken").ToString();

            var requestUrl = $"https://demo.docusign.net/restapi/v2.1/accounts/{accountId}/envelopes/{envelopeId}/recipients/1/signature_image";
            var signatureResponseData = await _client.Get(requestUrl, accessToken, "application/pdf", "");
            //var responsePdfData = await pdfResponseData.Content.ReadAsStringAsync();
            byte[] imageByte;
            Stream signatureImage = await signatureResponseData.Content.ReadAsStreamAsync();
            using (MemoryStream ms = new MemoryStream())
            {
                signatureImage.CopyTo(ms);
                imageByte = ms.ToArray();
            }
            string imreBase64Data = Convert.ToBase64String(imageByte);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view
            return  imgDataURL.ToString();
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
