using Moq;
using SendGrid.Helpers.Mail;
using System.Net;
using SendGrid;

public class EmailServiceTest
{
    [Fact]
    public async Task EnviarEmailAsync_DeveChamarSendEmailAsyncComSucesso()
    {
        // Arrange
        var mockSendGridClient = new Mock<ISendGridClient>();

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("OK")
        };

        var mockResponse = new Mock<Response>(httpResponseMessage.StatusCode, httpResponseMessage.Content, httpResponseMessage.Headers);

        mockSendGridClient.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var emailService = new EmailService(mockSendGridClient.Object);

        var toEmail = "test@dominio.com";
        var subject = "Test Subject";
        var content = "Test Content";

        // Act
        await emailService.EnviarEmailAsync(toEmail, subject, content);

        // Assert
        mockSendGridClient.Verify(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task EnviarEmailAsync_DeveTratarErroAoEnviarEmail()
    {
        // Arrange
        var mockSendGridClient = new Mock<ISendGridClient>();

        var statusCode = HttpStatusCode.InternalServerError; 
        var responseBody = new StringContent("Internal Server Error"); 
        var responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = responseBody
        };
        
        responseMessage.Headers.Add("X-Error", "Something went wrong");

        var mockResponse = new Mock<Response>(statusCode, responseBody, responseMessage.Headers);

        mockSendGridClient.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var emailService = new EmailService(mockSendGridClient.Object);

        var toEmail = "test@dominio.com";
        var subject = "Test Subject";
        var content = "Test Content";

        // Act & Assert
        await emailService.EnviarEmailAsync(toEmail, subject, content);

        mockSendGridClient.Verify(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    
    [Fact]
    public async Task EnviarEmailAsync_DeveTratarErroAoEnviarEmailComExcecao()
    {
        // Arrange
        var mockSendGridClient = new Mock<ISendGridClient>();

        mockSendGridClient.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro ao enviar e-mail"));

        var emailService = new EmailService(mockSendGridClient.Object);

        var toEmail = "test@dominio.com";
        var subject = "Test Subject";
        var content = "Test Content";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => emailService.EnviarEmailAsync(toEmail, subject, content));

        Assert.Equal("Erro ao enviar e-mail", exception.Message);
        mockSendGridClient.Verify(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }



}