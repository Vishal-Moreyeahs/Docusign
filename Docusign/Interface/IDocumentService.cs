
using Docusign.Models;

namespace Docusign.Interface
{
    public interface IDocumentService
    {
        byte[] ConvertHtmlToPdf(string html);
        string PopulateHtmlWithDynamicValues(string html, User  user);

    }
}
