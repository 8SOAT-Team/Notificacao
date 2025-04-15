using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Notificacao;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly string _queueUrl;
    private readonly string _sendGridApiKey;
    
    public EmailService(IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _queueUrl = _configuration["Aws:SqsQueueUrl"] ?? throw new ArgumentNullException(nameof(_queueUrl));
        _sendGridApiKey = _configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException(nameof(_sendGridApiKey));
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 20
            };

            var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            if (receiveMessageResponse.Messages != null && receiveMessageResponse.Messages.Any())
            {
                foreach (var message in receiveMessageResponse.Messages)
                {
                    try
                    {
                        var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message.Body);

                        if (emailMessage != null)
                        {
                            await SendEmailAsync(emailMessage);
                            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                        }
                        else
                        {
                            Console.WriteLine($"Mensagem SQS vazia ou inválida. Corpo da mensagem: {message.Body}");
                        }

                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Erro ao desserializar JSON: {ex.Message}, Corpo da mensagem: {message.Body}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar mensagem SQS: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Nenhuma mensagem recebida do SQS.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
    
    protected async Task SendEmailAsync(EmailMessage emailMessage)
    {
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress("feehvecch@gmail.com", "Fast Video");
        var to = new EmailAddress(emailMessage.ToEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, emailMessage.Subject, emailMessage.Content, emailMessage.Content);
        await client.SendEmailAsync(msg);
    }
}