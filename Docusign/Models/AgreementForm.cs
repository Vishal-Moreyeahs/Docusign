namespace Docusign.Models
{
    public class AgreementForm
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool SignHere { get; set; } = false;
        public bool InitialHere { get; set; } = false;
    }

}
