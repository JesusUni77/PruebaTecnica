namespace Versalink.Modelo
{
    public class EmailModel
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string RecipientName { get; set; }
        public string Recipient { get; set; }
        public string? AttachmentBase64 { get; set; } // Base64 string of the attachment
        public string? AttachmentFileName { get; set; } // Name of the attachment file
        public string? Idioma { get; set; }
    }
}
