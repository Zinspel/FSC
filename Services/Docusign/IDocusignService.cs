namespace Docusign.Service
{
    public interface IDocusignService
    {
        string CreateEnvelope(string signerEmail, string signerName,string documentBase64);
        string CreateRecipientView(string signerEmail, string signerName, string envelopeId);
    }
}