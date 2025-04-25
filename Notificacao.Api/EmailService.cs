using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notificacao
{
    public class EmailService : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _configuration;
        private readonly string _queueUrl;
        private readonly string _sendGridApiKey;
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly string _userPoolId;

        public EmailService(IAmazonSQS sqsClient, IConfiguration configuration, IAmazonCognitoIdentityProvider cognitoClient)
        {
            _sqsClient = sqsClient;
            _configuration = configuration;
            _queueUrl = _configuration["Aws:SqsQueueUrl"] ?? throw new ArgumentNullException(nameof(_queueUrl));
            _sendGridApiKey = _configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException(nameof(_sendGridApiKey));
            _cognitoClient = cognitoClient;
            _userPoolId = _configuration["Aws:CognitoUserPoolId"] ?? throw new ArgumentNullException(nameof(_userPoolId));
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
                        await ProcessSqsMessageAsync(message, stoppingToken);
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma mensagem recebida do SQS.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessSqsMessageAsync(Message sqsMessage, CancellationToken stoppingToken)
        {
            try
            {
                var processingResult = JsonSerializer.Deserialize<ProcessingResult>(sqsMessage.Body);

                if (processingResult != null)
                {
                    string userEmail = await GetUserEmailFromCognito(processingResult.RequestProcessId.ToString());

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        (string subject, string body) = ProcessingResultHandler.GetEmailContent(processingResult.UploadResultSecceeded);
                        await SendEmailAsync(userEmail, subject, body);
                        await _sqsClient.DeleteMessageAsync(_queueUrl, sqsMessage.ReceiptHandle, stoppingToken);
                    }
                    else
                    {
                        Console.WriteLine($"Nenhum e-mail encontrado no Cognito para RequestProcessId: {processingResult.RequestProcessId}");
                        await _sqsClient.DeleteMessageAsync(_queueUrl, sqsMessage.ReceiptHandle, stoppingToken);
                    }
                }
                else
                {
                    Console.WriteLine($"Mensagem SQS vazia ou inválida. Corpo da mensagem: {sqsMessage.Body}");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro ao desserializar JSON: {ex.Message}, Corpo da mensagem: {sqsMessage.Body}");
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine($"Usuário não encontrado no Cognito: {ex.Message}");
                await _sqsClient.DeleteMessageAsync(_queueUrl, sqsMessage.ReceiptHandle, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem SQS: {ex.Message}");
            }
        }

        private async Task<string> GetUserEmailFromCognito(string requestProcessId) 
        {
            try
            {
                var request = new ListUsersRequest
                {
                    UserPoolId = _userPoolId,
                    Filter = $"sub = \"{requestProcessId}\""
                };

                var response = await _cognitoClient.ListUsersAsync(request);

                if (response.Users.Count > 0)
                {
                    var user = response.Users[0];
                    foreach (var attr in user.Attributes)
                    {
                        if (attr.Name == "email")
                            return attr.Value;
                    }
                }
            }
            catch (UserNotFoundException)
            {
                Console.WriteLine($"Usuário com RequestProcessId {requestProcessId} não encontrado no Cognito.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar usuário no Cognito: {ex.Message}");
                return null;
            }

            return null;
        }

        protected async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("feehvecch@gmail.com", "Fast Video");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            await client.SendEmailAsync(msg);
        }
    }
}