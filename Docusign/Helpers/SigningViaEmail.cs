using Docusign.Models;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System.Text;

namespace Docusign.Helpers
{
    public static class SigningViaEmail
    {
        public static dynamic SendEnvelopeViaEmail(byte[] pdfDoc,string signerEmail, string signerName, string ccEmail, string ccName, string accessToken, 
                                                        string basePath , string accountId, string docPdf, string envStatus)
        {
            //ds-snippet-start:eSign2Step3
            EnvelopeDefinition env = MakeEnvelope(signerEmail, signerName, ccEmail, ccName, docPdf, envStatus, pdfDoc);
            var docuSignClient = new DocuSignClient(basePath);
            docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

            EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, env);

            var viewRequest = MakeRecipientViewRequest(signerEmail, signerName, "https://localhost:7269/Home/SecondPage");
            ViewUrl results1 = envelopesApi.CreateRecipientView(accountId, results.EnvelopeId, viewRequest);
            var url = results1.Url;

            return new { EnvelopeId = results.EnvelopeId, SignedUrl = url, EnvelopesApiData = envelopesApi };
        }

        public static RecipientViewRequest MakeRecipientViewRequest(string signerEmail, string signerName,
                               string dsReturnUrl)
        {
            // Data for this method
            // signerEmail
            // signerName
            // dsReturnUrl
            // signerClientId -- class global

            RecipientViewRequest viewRequest = new RecipientViewRequest
            {
                // Set the url where you want the recipient to go once they are done signing
                // should typically be a callback route somewhere in your app.
                ReturnUrl = dsReturnUrl,

                // How has your app authenticated the user? In addition to your app's
                // authentication, you can include authenticate steps from DocuSign.
                // Eg, SMS authentication
                AuthenticationMethod = "none",

                // Recipient information must match embedded recipient info
                // we used to create the envelope.
                Email = signerEmail,
                UserName = signerName,
                ClientUserId = "123456"
            };

            // DocuSign recommends that you redirect to DocuSign for the
            // Signing Ceremony. There are multiple ways to save state.

            return viewRequest;
        }

        //public static RecipientViewRequest MakeRecipientViewRequest(string signerEmail, string signerName, string returnUrl, string signerClientId, string pingUrl = null)
        //{
        //    // Data for this method
        //    // signerEmail
        //    // signerName
        //    // dsPingUrl -- class global
        //    // signerClientId -- class global
        //    // dsReturnUrl -- class global
        //    RecipientViewRequest viewRequest = new RecipientViewRequest();

        //    // Set the URL where you want the recipient to go once they're done signing
        //    // It should typically be a callback route somewhere in your app
        //    // The query parameter is included as an example of how
        //    // to save/recover state information during the redirect to
        //    // the DocuSign signing session. It's usually better to use
        //    // the session mechanism of your web framework. Query parameters
        //    // can be changed or spoofed very easily.
        //    viewRequest.ReturnUrl = returnUrl + "?state=123";

        //    // How does your app verify the user's authentication? Additionally,
        //    // you can integrate authentication steps from DocuSign alongside
        //    // your app's own authentication process
        //    // Eg, SMS authentication
        //    viewRequest.AuthenticationMethod = "none";

        //    // Recipient information must match embedded recipient info
        //    // we used to create the envelope
        //    viewRequest.Email = signerEmail;
        //    viewRequest.UserName = signerName;
        //    viewRequest.ClientUserId = signerClientId;

        //    // DocuSign recommends that you redirect to DocuSign for the
        //    // signing session. There are multiple ways to save state.
        //    // To maintain your application's session, use the pingUrl
        //    // parameter. It causes the DocuSign signing session web page
        //    // (not the DocuSign server) to send pings via AJAX to your
        //    // app.
        //    // NOTE: The pings will only be sent if the pingUrl is an https address
        //    if (pingUrl != null)
        //    {
        //        viewRequest.PingFrequency = "600"; // seconds
        //        viewRequest.PingUrl = pingUrl; // optional setting
        //    }

        //    // The FrameAncestors should include the links https://apps-d.docusign.com
        //    // for demo environments and https://apps.docusign.com for prod environments,
        //    // and the site where the document should be embedded.
        //    // The site must have a valid SSL protocol for the embed to work.
        //    // MessageOrigins includes the demo or prod link and only
        //    // takes a single string
        //    viewRequest.FrameAncestors = new List<string> { "https://localhost:7269", "https://apps-d.docusign.com" };
        //    viewRequest.MessageOrigins = new List<string> { "https://apps-d.docusign.com" };

        //    return viewRequest;
        //}


        public static EnvelopeDefinition MakeEnvelope(string signerEmail, string signerName, string ccEmail, string ccName, string docPdf, string envStatus, byte[] pdfDoc)
        {
            //string doc2DocxBytes = Convert.ToBase64String(System.IO.File.ReadAllBytes(docDocx));
            //string doc3PdfBytes = Convert.ToBase64String(System.IO.File.ReadAllBytes("C:\\Users\\user\\Downloads\\Docusign\\Docusign\\Docusign\\World_Wide_Corp_lorem.pdf"));
            string docs = Convert.ToBase64String(pdfDoc);
            EnvelopeDefinition env = new EnvelopeDefinition();
            env.EmailSubject = "Please sign this document set";

            // Create document objects, one per document
            //Document doc1 = new Document();
            //string b64 = Convert.ToBase64String(Document1(signerEmail, signerName, ccEmail, ccName));
            //doc1.DocumentBase64 = b64;
            //doc1.Name = "Order acknowledgement"; // can be different from actual file name
            //doc1.FileExtension = "html"; // Source data format. Signed docs are always pdf.
            //doc1.DocumentId = "1"; // a label used to reference the doc
            Document doc2 = new Document
            {
                DocumentBase64 = docs,
                Name = "Signature Demo", // can be different from actual file name
                FileExtension = "html",
                DocumentId = "1",
            };

            // The order in the docs array determines the order in the envelope
            env.Documents = new List<Document> { doc2 };

            // create a signer recipient to sign the document, identified by name and email
            // We're setting the parameters via the object creation
            Signer signer1 = new Signer
            {
                Email = signerEmail,
                Name = signerName,
                RecipientId = "1",
                RoutingOrder = "1",
                ClientUserId = "123456"
            };
            //CarbonCopy cc1 = new CarbonCopy
            //{
            //    Email = ccEmail,
            //    Name = ccName,
            //    RecipientId = "2",
            //    RoutingOrder = "2",
            //};

            SignHere signHere1 = new SignHere
            {
                AnchorString = "**signature_1**",
                AnchorUnits = "pixels",
                AnchorYOffset = "10",
                AnchorXOffset = "20",
            };

            //SignHere signHere2 = new SignHere
            //{
            //    AnchorString = "signhere",
            //    AnchorUnits = "pixels",
            //    AnchorYOffset = "10",
            //    AnchorXOffset = "20",
            //};

            // Tabs are set per recipient / signer
            Tabs signer1Tabs = new Tabs
            {
                SignHereTabs = new List<SignHere> { signHere1 },
            };
            signer1.Tabs = signer1Tabs;

            // Add the recipients to the envelope object
            Recipients recipients = new Recipients
            {
                Signers = new List<Signer> { signer1 },
                //CarbonCopies = new List<CarbonCopy> { cc1 },
            };
            env.Recipients = recipients;

            // Request that the envelope be sent by setting |status| to "sent".
            // To request that the envelope be created as a draft, set to "created"
            env.Status = envStatus;

            return env;
            //ds-snippet-end:eSign2Step2
        }

        public static byte[] Document1(string signerEmail, string signerName, string ccEmail, string ccName)
        {
            // Data for this method
            // signerEmail
            // signerName
            // ccEmail
            // ccName
            return Encoding.UTF8.GetBytes(
            " <!DOCTYPE html>\n" +
                "    <html>\n" +
                "        <head>\n" +
                "          <meta charset=\"UTF-8\">\n" +
                "        </head>\n" +
                "        <body style=\"font-family:sans-serif;margin-left:2em;\">\n" +
                "        <h1 style=\"font-family: 'Trebuchet MS', Helvetica, sans-serif;\n" +
                "            color: darkblue;margin-bottom: 0;\">World Wide Corp</h1>\n" +
                "        <h2 style=\"font-family: 'Trebuchet MS', Helvetica, sans-serif;\n" +
                "          margin-top: 0px;margin-bottom: 3.5em;font-size: 1em;\n" +
                "          color: darkblue;\">Order Processing Division</h2>\n" +
                "        <h4>Ordered by " + signerName + "</h4>\n" +
                "        <p style=\"margin-top:0em; margin-bottom:0em;\">Email: " + signerEmail + "</p>\n" +
                "        <p style=\"margin-top:0em; margin-bottom:0em;\">Copy to: " + ccName + ", " + ccEmail + "</p>\n" +
                "        <p style=\"margin-top:3em;\">\n" +
                "  Candy bonbon pastry jujubes lollipop wafer biscuit biscuit. Topping brownie sesame snaps sweet roll pie. Croissant danish biscuit soufflé caramels jujubes jelly. Dragée danish caramels lemon drops dragée. Gummi bears cupcake biscuit tiramisu sugar plum pastry. Dragée gummies applicake pudding liquorice. Donut jujubes oat cake jelly-o. Dessert bear claw chocolate cake gummies lollipop sugar plum ice cream gummies cheesecake.\n" +
                "        </p>\n" +
                "        <!-- Note the anchor tag for the signature field is in white. -->\n" +
                "        <h3 style=\"margin-top:3em;\">Agreed: <span style=\"color:white;\">**signature_1**/</span></h3>\n" +
                "        </body>\n" +
                "    </html>");
        }
    }

}
