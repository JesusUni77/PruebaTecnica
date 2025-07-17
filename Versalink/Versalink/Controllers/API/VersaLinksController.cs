using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Net;
using Versalink.Modelo;
using Versalink.Server;

namespace Versalink.Controllers.API
{
    public class VersaLinksController : Controller
    {
        public dynamic objetoDinamico = new JObject();
        private readonly EmailSettings _emailSettings;

        public VersaLinksController(IConfiguration config)
        {
            EmailSettings emailSettings = new EmailSettings();

            emailSettings.From = config.GetSection("EmailSettings").GetSection("From").Value;
            emailSettings.Username = config.GetSection("EmailSettings").GetSection("Username").Value;
            emailSettings.Password = config.GetSection("EmailSettings").GetSection("Password").Value;
            emailSettings.Host = config.GetSection("EmailSettings").GetSection("Host").Value;
            emailSettings.Port = config.GetSection("EmailSettings").GetSection("Port").Value;
            emailSettings.CC = config.GetSection("EmailSettings").GetSection("CC").Value;
            emailSettings.AppName = config.GetSection("EmailSettings").GetSection("AppName").Value;
            _emailSettings = emailSettings;
        }

        [HttpPost]
        [Route("{sp}")]

        public async Task<ActionResult<string>> Execute([FromRoute] string sp, [FromBody] dynamic parameters)
        {
            try
            {
                var jsonParameters = JsonConvert.DeserializeObject<dynamic>(parameters.ToString());
                ConexionSql conexion = new ConexionSql();
                objetoDinamico.Data = await conexion.EjecutarProcedimientoAlmacenado(sp, jsonParameters.ToString());
            }
            catch (Exception ex)
            {
                objetoDinamico.Data = "Error";
                objetoDinamico.Source = ex.Source != null ? ex.Source : "Source no definido";
                objetoDinamico.Error = ex.Message;
                return Ok(objetoDinamico.ToString());

            }
            return Ok(objetoDinamico.Data.Value);
        }
        [HttpPost]
        [Route("SendEmail")]

        public async Task<ActionResult<string>> SendEmail([FromBody] EmailModel emailModel)
        {
            using (var client = new System.Net.Mail.SmtpClient())
            {
                client.Host = _emailSettings.Host;
                client.Port = int.Parse(_emailSettings.Port);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.EnableSsl = false;

                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                using (var message = new MailMessage(
                    from: new MailAddress(_emailSettings.Username, _emailSettings.AppName),
                    to: new MailAddress(emailModel.Recipient, emailModel.RecipientName)
                    ))
                {

                    message.Subject = emailModel.Subject;
                    message.IsBodyHtml = true;
                    message.Body = emailModel.Content;
                    // message.CC.Add(new MailAddress(_emailSettings.CC, _emailSettings.AppName));

                    // Add attachment if provided
                    if (!string.IsNullOrEmpty(emailModel.AttachmentBase64) && !string.IsNullOrEmpty(emailModel.AttachmentFileName))
                    {
                        byte[] fileBytes = Convert.FromBase64String(emailModel.AttachmentBase64);
                        using (var stream = new MemoryStream(fileBytes))
                        {

                            var attachment = new Attachment(stream, emailModel.AttachmentFileName, "application/pdf");
                            message.Attachments.Add(attachment);
                            //solo mandar cc, si se envia archivo adjunto....
                            message.CC.Add(new MailAddress(_emailSettings.CC, _emailSettings.AppName));

                            client.Send(message);
                        }
                    }
                    else
                    {
                        client.Send(message);
                    }
                }
            }
            return Ok("Email Enviado!");

        }
    }
}
