using System.Collections.Generic;

namespace App.Models
{
    public class EmailModel
    {
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<EmailAttachment> Attachments { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}