using NLog;
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using VTACheckClock.Models;
using MimeKit;
using MailKit.Security;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using LegacySmtpClient = System.Net.Mail.SmtpClient;

namespace VTACheckClock.Services
{
    class EmailSenderHandler
    {
        private static readonly Logger log = LogManager.GetLogger("app_logger");

        /// <summary>
        /// Asynchronously sends an email message with the specified subject and HTML body to the configured recipients.
        /// </summary>
        /// <remarks>If the mail configuration cannot be retrieved, the email is not sent and the method
        /// returns immediately. Any exceptions that occur during the sending process are logged for diagnostic purposes
        /// but are not propagated to the caller.</remarks>
        /// <param name="subject">The subject line of the email message.</param>
        /// <param name="body">The HTML content to include in the body of the email message.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public static async Task SendEmailAsync(string subject, string body)
        {
            if (!GetMailConfig(out string host, out int port, out string username, out string password, out string recipients))
            {
                return;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("VTSoftware", username));
                SetToAddress(message, recipients);
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = 30000;

                await client.ConnectAsync(host, port, SecureSocketOptions.Auto);

                // Autenticar
                client.AuthenticationMechanisms.Remove("XOAUTH2"); // Deshabilitar OAuth si no se usa
                await client.AuthenticateAsync(username, password);

                // Enviar
                await client.SendAsync(message);

                // Desconectar
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"Error enviando correo (MailKit): {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the mail configuration settings required for sending emails, including the SMTP server host, port,
        /// authentication credentials, and recipient addresses.
        /// </summary>
        /// <remarks>If the configuration is incomplete or invalid, a warning is logged detailing the
        /// missing or incorrect settings. All output parameters are set to empty or default values when the
        /// configuration is not valid.</remarks>
        /// <param name="host">When the method returns <see langword="true"/>, contains the SMTP server host address used for sending
        /// emails. Otherwise, set to an empty string.</param>
        /// <param name="port">When the method returns <see langword="true"/>, contains the port number for the SMTP server. Must be a
        /// valid integer greater than zero. Otherwise, set to zero.</param>
        /// <param name="username">When the method returns <see langword="true"/>, contains the username for authenticating with the SMTP
        /// server. Otherwise, set to an empty string.</param>
        /// <param name="password">When the method returns <see langword="true"/>, contains the password for authenticating with the SMTP
        /// server. Otherwise, set to an empty string.</param>
        /// <param name="recipients">When the method returns <see langword="true"/>, contains a comma-separated list of email addresses to which
        /// emails will be sent. Otherwise, set to an empty string.</param>
        /// <returns>true if the mail configuration is valid and enabled; otherwise, false.</returns>
        private static bool GetMailConfig(out string host, out int port, out string username, out string password, out string recipients)
        {
            var config = RegAccess.GetMainSettings() ?? new MainSettings();

            host = config.MailServer ?? "";
            bool validPort = int.TryParse(config?.MailPort, out port);
            username = config.MailUser ?? "";
            password = config.MailPass ?? "";
            bool IsEnabled = config.MailEnabled;
            recipients = config.MailRecipient ?? "";

            bool hasHost = !string.IsNullOrWhiteSpace(host);
            bool hasPort = validPort && port != 0;
            bool hasUser = !string.IsNullOrWhiteSpace(username);
            bool hasPass = !string.IsNullOrWhiteSpace(password);
            bool hasRecipients = !string.IsNullOrEmpty(recipients);

            bool isValid = hasHost && hasPort && hasUser && hasPass && hasRecipients && IsEnabled;

            if (!isValid && IsEnabled)
            {
                var detalles = $"Host='{host}', Port='{config?.MailPort}', User='{username}', Recipients='{recipients}', Habilitado={IsEnabled}";
                var faltantes = new StringBuilder();
                if (!IsEnabled) faltantes.Append("MailEnabled=false; ");
                if (!hasHost) faltantes.Append("Host vacío; ");
                if (!hasPort) faltantes.Append("Puerto inválido; ");
                if (!hasUser) faltantes.Append("Usuario vacío; ");
                if (!hasPass) faltantes.Append("Contraseña vacía; ");
                if (!hasRecipients) faltantes.Append("Destinatarios vacíos; ");

                log.Warn($"Configuración SMTP incompleta/incorrecta. {detalles}. Falta: {faltantes.ToString().Trim()}");
            }

            return isValid;
        }

        private static void SetToAddress(MimeMessage message, string emails)
        {
            char[] separators = [',', ';'];
            foreach (var email in SplitEmailsByDelimiter(emails, separators))
            {
                var em = email.Trim();
                try
                {
                    if (MailboxAddress.TryParse(em, out var mailboxAddress))
                    {
                        message.To.Add(mailboxAddress);
                    }
                    else
                    {
                        log.Warn($"Dirección de correo inválida ignorada: '{em}'");
                    }
                }
                catch (Exception ex)
                {
                    log.Warn(ex, $"Dirección de correo inválida ignorada: '{em}'");
                }
            }
        }

        [Obsolete("Use MailKit implementation instead. This method relies on System.Net.Mail.SmtpClient which is deprecated.")]
        private static bool SetMailConfig_Legacy(LegacySmtpClient oSmtpClient, ref string recipients)
        {
            if (GetMailConfig(out string host, out int port, out string username, out string password, out string configRecipients))
            {
                recipients = configRecipients;

                oSmtpClient.Host = host;
                oSmtpClient.Port = port;
                oSmtpClient.Credentials = new NetworkCredential(username, password);
                oSmtpClient.UseDefaultCredentials = false;
                oSmtpClient.EnableSsl = true;
                oSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                return true;
            }

            return false;
        }

        [Obsolete("Use MailKit implementation instead.")]
        public static async Task SendEmailAsync_Legacy(string subject, string body)
        {
            try {
                using LegacySmtpClient oSmtpClient = new();
                string recipients = "";

                if (!SetMailConfig_Legacy(oSmtpClient, ref recipients)) {
                    return;
                }

                oSmtpClient.Timeout = 30000; // 30s

                NetworkCredential? credentials = oSmtpClient?.Credentials as NetworkCredential;

                var oMailMessage = new MailMessage {
                    From = new MailAddress(credentials?.UserName ?? "", "VTSoftware")
                };

                SetToAddress_Legacy(ref oMailMessage, recipients);

                oMailMessage.Subject = subject;
                oMailMessage.SubjectEncoding = Encoding.UTF8;
                oMailMessage.Body = body;
                oMailMessage.BodyEncoding = Encoding.UTF8;
                oMailMessage.IsBodyHtml = true;

                await oSmtpClient.SendMailAsync(oMailMessage);
            }
            catch (SmtpFailedRecipientsException ex)
            {
                foreach (var inner in ex.InnerExceptions)
                {
                    log.Warn(inner, $"Fallo SMTP para destinatario '{inner.FailedRecipient}' (Status: {inner.StatusCode})");
                }
                log.Warn(ex, "Error SMTP: múltiples destinatarios fallidos");
            }
            catch (SmtpFailedRecipientException ex)
            {
                log.Warn(ex, $"Fallo SMTP para destinatario '{ex.FailedRecipient}' (Status: {ex.StatusCode})");
            }
            catch (SmtpException smtpEx) {
                log.Warn(smtpEx, $"Error SMTP al enviar el correo (Status: {smtpEx.StatusCode}). Detalles: {smtpEx.ToString()}");
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"Error general al enviar el correo.");
            }
        }

        /// <summary>
        /// Add all the recipients to whom information will be sent
        /// </summary>
        /// <param name="oMailMessage"></param>
        /// <param name="emails">Mails concatenated in a text string with a special character.</param>
        [Obsolete("Use MailKit implementation instead.")]
        private static void SetToAddress_Legacy(ref MailMessage oMailMessage, string emails)
        {
            char[] separators = [',', ';'];
            //int added = 0;
            foreach (var email in SplitEmailsByDelimiter(emails, separators))
            {
                var em = email.Trim();
                try
                {
                    oMailMessage.To.Add(em);
                    //added++;
                }
                catch (Exception ex)
                {
                    log.Warn(ex, $"Dirección de correo inválida ignorada: '{em}'");
                }
            }
            //log.Debug($"Destinatarios agregados: {added}");
        }

        /// <summary>
        /// Useful to separate multiple concatenated emails.
        /// </summary>
        /// <param name="emails"></param>
        /// <param name="separators"></param>
        /// <returns></returns>
        public static string[] SplitEmailsByDelimiter(string emails, char[] separators)
        {
            return emails.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string BuildMessage(DataTable dt, DataTable dupPunches)
        {
            var emp_list = ExportDatatableToHtml(dt);
            var duplicatedPunches = ExportDatatableToHtml(dupPunches);
            var duplicatedHtml = string.IsNullOrEmpty(duplicatedPunches) ? "": $"""
                <p>Checadas Duplicadas</p>
                {duplicatedPunches}
            """;

            var body_msg = "<div style='box-sizing:border-box;background-color:#ffffff;color:#718096;height:100%;line-height:1.4;margin:0;padding:0;width:100%!important'>";
            body_msg +=
                $"""
                    <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="box-sizing:border-box;background-color:#edf2f7;margin:0;padding:0;width:100%;">
                    <tbody><tr>
                        <td align="center" style="box-sizing:border-box;">
                            <table width="100%" cellpadding="0" cellspacing="0" role="presentation" style="box-sizing:border-box;margin:0;padding:0;width:100%">
                                <tbody>
                                    <tr>
                                        <td width="100%" cellpadding="0" cellspacing="0" style="box-sizing:border-box;background-color:#edf2f7; border-bottom:1px solid #edf2f7;border-top:1px solid #edf2f7;margin:0;padding:0;width:100%;">
                                            <table align="center" width="570" cellpadding="0" cellspacing="0" role="presentation" style="box-sizing:border-box;background-color:#ffffff;border-color:#e8e5ef;border-radius:2px;border-width:1px;margin:0 auto;padding:0;width:570px">
                                                <tbody>
                                                    <tr>
                                                        <td style="box-sizing:border-box;max-width:100vw;padding:32px">
                                                            <h1 style="box-sizing:border-box;color:#3d4852;font-size:18px;font-weight:bold;margin-top:0;text-align:left">Estimado administrador,</h1>
                                                            <p style='box-sizing:border-box;font-size:16px;line-height:1.5em;margin-top:0;text-align:left'>
                                                                "Le escribo para informarle que algunos empleados no tienen checadas de entradas o salidas en la fecha actual. 
                                                                "Esto puede indicar que no han asistido al trabajo o que han tenido algún problema con el sistema de registro. 
                                                                "Le pido que revise la situación y tome las medidas necesarias.
                                                            </p> 
                                                            {emp_list}
                                                            {duplicatedHtml}
                                                           <p style='box-sizing:border-box;font-size:16px;line-height:1.5em;text-align:left'>Atentamente,</p>
                                                           <p style='box-sizing:border-box;font-size:16px;line-height:1.5em;margin-top:0;text-align:left'>El sistema de control de asistencia</p>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </td>
                    </tr>
                    </tbody>
                </table></div>
               """;

            return body_msg;
        }

        /// <summary>
        /// Funcion genérica para convertir un DataTable a una tabla HTML.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ExportDatatableToHtml(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "";

            StringBuilder strHTMLBuilder = new();
            strHTMLBuilder.Append("<table border='1' cellpadding='0' cellspacing='0' style='border:0;border-style:hidden;'>");
            strHTMLBuilder.Append("<thead>");
            strHTMLBuilder.Append("<tr>");
            foreach (DataColumn myColumn in dt.Columns)
            {
                strHTMLBuilder.Append("<th>");
                strHTMLBuilder.Append(myColumn.ColumnName);
                strHTMLBuilder.Append("</th>");
            }
            strHTMLBuilder.Append("</tr>");
            strHTMLBuilder.Append("</thead>");
            strHTMLBuilder.Append("<tbody>");

            foreach (DataRow myRow in dt.Rows)
            {
                strHTMLBuilder.Append("<tr>");
                foreach (DataColumn myColumn in dt.Columns)
                {
                    strHTMLBuilder.Append("<td style='padding: 3px;'>");
                    strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                    strHTMLBuilder.Append("</td>");
                }
                strHTMLBuilder.Append("</tr>");
            }
            strHTMLBuilder.Append("</tbody>");
            strHTMLBuilder.Append("</table>");

            string Htmltext = strHTMLBuilder.ToString();

            return Htmltext;
        }
    }
}
