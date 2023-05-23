using System.ComponentModel.DataAnnotations;

namespace PasswordManagerBlazor.Shared.Models
{
    public class EmailDetails
    {
       
        
        [Key]
        public int Id { get; set; }
        public string Recipient { get; set; }
        public string MsgBody { get; set; }
        public string Subject { get; set; }
        public string Attachment { get; set; }

        public EmailDetails()
        {
        }

        public EmailDetails(int id, string recipient, string msgBody, string subject, string attachment)
        {
            Id = id;
            Recipient = recipient;
            MsgBody = msgBody;
            Subject = subject;
            Attachment = attachment;
        }
    }
}
