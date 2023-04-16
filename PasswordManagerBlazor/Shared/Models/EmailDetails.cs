namespace PasswordManagerBlazor.Shared.Models
{
    public class EmailDetails
    {
        public string Recipient { get; set; }
        public string MsgBody { get; set; }
        public string Subject { get; set; }
        public string Attachment { get; set; }

        public EmailDetails()
        {
        }

        public EmailDetails(string recipient, string msgBody, string subject, string attachment)
        {
            Recipient = recipient;
            MsgBody = msgBody;
            Subject = subject;
            Attachment = attachment;
        }
    }
}
