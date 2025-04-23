using Moq;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Notificacao;
using SendGrid;

public class EmailServiceTest
{
    [Fact]
    public async Task ExecuteAsync_ReceivesMessage_SendsEmail_DeletesMessage()
    {
        // Arrange
        var mockSqsClient = new Mock<IAmazonSQS>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockCoginito = new Mock<IAmazonCognitoIdentityProvider>();
        
        mockConfiguration.Setup(config => config["Aws:SqsQueueUrl"]).Returns("sua_url_da_fila");
        mockConfiguration.Setup(config => config["SendGrid:ApiKey"]).Returns("sua_chave_api");
        mockConfiguration.Setup(config => config["Aws:CognitoUserPoolId"]).Returns("sua_user_pool_id");
        
        mockSqsClient.Setup(client => client.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceiveMessageResponse
            {
                Messages = new List<Message> { new Message { Body = "{ \"RequestProcessId\": \"123\", \"UploadResultSecceeded\": \"false\" }", ReceiptHandle = "handle" } }
            });
        
        mockSqsClient.Setup(client => client.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse());

        var service = new TestableEmailService(mockSqsClient.Object, mockConfiguration.Object, mockCoginito.Object);

        // Act
        await service.ExecuteAsync(CancellationToken.None);

        // Assert
        mockSqsClient.Verify(client => client.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        mockSqsClient.Verify(client => client.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    
    public class TestableEmailService : EmailService
    {
        protected IConfiguration Configuration { get; }
        protected IAmazonSQS SqsClient { get; }

        public TestableEmailService(IAmazonSQS sqsClient, IConfiguration configuration, IAmazonCognitoIdentityProvider coginito) : base(sqsClient, configuration, coginito)
        {
            Configuration = configuration;
            SqsClient = sqsClient;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueUrl = Configuration["Aws:SqsQueueUrl"];
            var sqsClient = SqsClient;
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 20
            };

            var receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var message in receiveMessageResponse.Messages)
            {
                try
                {
                    var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message.Body);
                    await SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Content);
                    await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                }
                catch (Exception ex)
                {
                    // Lidar com erros (log, etc.)
                    Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    throw new Exception("Erro ao enviar e-mail", ex);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }


}