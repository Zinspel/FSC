using Docusign.Service.Models;

namespace Docusign.Service
{
    public interface IDocusignService
    {
        string CreateEnvelope(string signerEmail, string signerName, string phoneNumber, string documentBase64, string accessCode);
        string CreateRecipientView(string signerEmail, string signerName, string envelopeId);
        void ResendEnvelope(string envelopeId);
        void UpdateRecipient(Contract contract);
    }
}