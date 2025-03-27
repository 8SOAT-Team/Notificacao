using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notificacao;

public class EmailService
{

    private static readonly string apiKey = "${API_KEY}";

    public static async Task EnviarEmailAsync(string toEmail, string subject, string content)
    {
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("feehvecch@gmail.com", "FastVideo");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);

        try
        {
            var response = await client.SendEmailAsync(msg);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine($"Status Code: {response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Erro ao enviar e-mail. Código de status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
        }
    }
}
