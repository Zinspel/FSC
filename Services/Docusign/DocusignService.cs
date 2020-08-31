using AuditQualification.Service;
using DocuSign.eSign.Model;
using System.Collections.Generic;


namespace Docusign.Service
{
    public class DocusignService : IDocusignService
    {
        private IDocusignClient _client;
        private readonly string signerClientId = "1000";
        private readonly string dsReturnUrl = "https://auditqualification.azurewebsite.com/Docusign/dsreturn";

        public DocusignService(IDocusignClient docusignClient)
        {
            _client = docusignClient;
        }
        public string CreateEnvelope(string signerEmail, string signerName, string documentBase64)
        {
            var envelope = MakeEnvelope(signerEmail, signerName, documentBase64);
            return _client.CreateEnvelope(envelope);
        }

        public string CreateRecipientView(string signerEmail, string signerName, string envelopeId)
        {
            var viewRequest = new RecipientViewRequest
            {
                ReturnUrl = dsReturnUrl + "?state=123&envelopeId=" + envelopeId,

                AuthenticationMethod = "none",

                Email = signerEmail,
                UserName = signerName,
                ClientUserId = signerClientId,
            };

            return _client.CreateRecipientView(envelopeId, viewRequest);
        }



        private EnvelopeDefinition MakeEnvelope(string signerEmail, string signerName, string documentBase64)
        {

            var envelopeDefinition = new EnvelopeDefinition()
            {
                EmailSubject = "Please sign this document"
            };

            var doc = new Document()
            {
                DocumentBase64 = documentBase64,
                Name = $"Contract_{signerName}",
                FileExtension = "pdf",
                DocumentId = "3"
            };

            envelopeDefinition.Documents = new List<Document> { doc };
            var signer = new Signer
            {
                Email = signerEmail,
                Name = signerName,
                ClientUserId = signerClientId,
                RecipientId = "1",
            };

            var signHere = new SignHere
            {
                AnchorString = "/signature/",
                AnchorUnits = "pixels"
            };

            var dateSigned = new DateSigned
            {
                AnchorString = "/datesigned/",
                AnchorUnits = "pixels"
            };

            var placeSigned = new Text
            {
                AnchorString = "/placesigned/",
                AnchorUnits = "pixels",
                Width = "100"
            };

            var signerTabs = new Tabs
            {
                SignHereTabs = new List<SignHere> { signHere },
                DateSignedTabs = new List<DateSigned> { dateSigned },
                TextTabs = new List<Text> { placeSigned }
            };

            signer.Tabs = signerTabs;

            var recipients = new Recipients
            {
                Signers = new List<Signer> { signer }
            };

            var envelope_events = new List<EnvelopeEvent>()
            {
                new EnvelopeEvent
                {
                    EnvelopeEventStatusCode = "completed"
                }
            };

            var event_notification = new EventNotification
            {
                Url = "https://auditqualification.azurewebsite.com/Docusign/connectwebhook",
                LoggingEnabled = "true",
                RequireAcknowledgment = "true",
                UseSoapInterface = "false",
                IncludeCertificateWithSoap = "false",
                SignMessageWithX509Cert = "false",
                IncludeDocuments = "true",
                IncludeEnvelopeVoidReason = "true",
                IncludeTimeZone = "true",
                IncludeSenderAccountAsCustomField = "true",
                IncludeDocumentFields = "true",
                IncludeCertificateOfCompletion = "true",
                EnvelopeEvents = envelope_events
            };

            envelopeDefinition.EventNotification = event_notification;
            envelopeDefinition.Recipients = recipients;

            envelopeDefinition.Status = "sent";

            return envelopeDefinition;
        }

    }
}
