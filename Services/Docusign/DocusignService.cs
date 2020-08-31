using System;
using System.Collections.Generic;
using System.Linq;

using Docusign.Service.Models;

using DocuSign.eSign.Model;

using VerinPortaal.Service;

namespace Docusign.Service
{
    public class DocusignService : IDocusignService
    {
        private readonly string signerClientId = "1000";
        private readonly string dsReturnUrl = VerinEnvironment.UrlMijnPortaal() + "/mvc/Docusign/dsreturn";

        public string CreateEnvelope(string signerEmail, string signerName, string phoneNumber, string documentBase64, string accessCode)
        {
            var _client = new DocusignClient();
            var envelope = MakeEnvelope(signerEmail, signerName, phoneNumber, documentBase64, accessCode);
            return _client.CreateEnvelope(envelope);
        }

        public string CreateRecipientView(string signerEmail, string signerName, string envelopeId)
        {
            var _client = new DocusignClient();
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

        public void ResendEnvelope(string envelopeId)
        {
            var _client = new DocusignClient();
            _client.ResendEnvelope(envelopeId);
        }

        public void UpdateRecipient(Contract contract)
        {
            var _client = new DocusignClient();

            var recipients = _client.GetRecipients(contract.EnvelopeId);

            var signer = recipients.Signers.FirstOrDefault();
            if (signer == null) throw new Exception($"Signer is null with contractId: {contract.ContractId}, envelopeId: {contract.EnvelopeId}");

            if (!string.IsNullOrEmpty(contract.Mobiel))
            {
                var smsAuth = new RecipientSMSAuthentication
                {
                    SenderProvidedNumbers = new List<string>() { contract.Mobiel }
                };
                signer.IdCheckConfigurationName = "SMS Auth $";
                signer.SmsAuthentication = smsAuth;

                signer.AccessCode = "";
            }
            else
            {
                _client.DeleteRecipient(contract.EnvelopeId, signer.RecipientId);

                signer.AccessCode = contract.AccessCode;

                signer.SmsAuthentication = null;
                signer.IdCheckConfigurationName = null;
            }

            signer.Email = contract.Email;
            signer.Name = contract.SignerName;

            recipients.Signers = new List<Signer>() { signer };

            _client.UpdateRecipients(contract.EnvelopeId, recipients);
        }

        private EnvelopeDefinition MakeEnvelope(string signerEmail, string signerName, string phoneNumber, string documentBase64, string accessCode)
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

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var smsAuth = new RecipientSMSAuthentication
                {
                    SenderProvidedNumbers = new List<string>() { phoneNumber }
                };
                signer.IdCheckConfigurationName = "SMS Auth $";
                signer.SmsAuthentication = smsAuth;
            }
            else
            {
                signer.AccessCode = accessCode;
            }

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
                Url = VerinEnvironment.UrlMijnPortaal() + "/mvc/docusign/connectwebhook",
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
