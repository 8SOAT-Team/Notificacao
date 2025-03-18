namespace Notificacao;

using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _emailDe = "seuemail@dominio.com";
    private readonly string _senha = "senha";

    public async Task EnviarEmailAsync(string para, string assunto, string corpo)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Fast Vídeo", _emailDe));
        message.To.Add(new MailboxAddress("", para));
        message.Subject = assunto;

        var body = new TextPart("plain") { Text = corpo };
        message.Body = body;

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailDe, _senha);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
