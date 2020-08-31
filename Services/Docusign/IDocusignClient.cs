using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;

namespace AuditQualification.Service
{
    public interface IDocusignClient
    {
        OAuth.UserInfo.Account Account { get; }

        string CreateEnvelope(EnvelopeDefinition envelope);
        string CreateRecipientView(string envelopeId, RecipientViewRequest viewRequest);
    }
}