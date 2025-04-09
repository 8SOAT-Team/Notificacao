using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService
{
    private readonly ISendGridClient _sendGridClient;
    
    public EmailService(ISendGridClient sendGridClient)
    {
        _sendGridClient = sendGridClient ?? throw new ArgumentNullException(nameof(sendGridClient));
    }

    private static readonly string apiKey = "${API_KEY}";

    public async Task EnviarEmailAsync(string toEmail, string subject, string content)
    {
        var client = _sendGridClient ?? new SendGridClient(apiKey);

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
            throw new Exception("Erro ao enviar e-mail", ex);
        }
    }
}