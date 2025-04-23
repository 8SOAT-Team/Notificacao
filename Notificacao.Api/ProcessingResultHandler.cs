namespace Notificacao;

public class ProcessingResultHandler
{
    private const string EmailSubjectSuccess = "Upload Concluído com Sucesso";
    private const string EmailSubjectFailure = "Falha no Upload";
    private const string EmailBodySuccess = "Seu upload foi concluído com sucesso!";
    private const string EmailBodyFailure = "Houve uma falha ao processar seu upload. Por favor, tente novamente ou entre em contato com o suporte.";

    public static (string subject, string body) GetEmailContent(bool uploadResultSecceeded)
    {
        if (uploadResultSecceeded)
        {
            return (EmailSubjectSuccess, EmailBodySuccess);
        }
        else
        {
            return (EmailSubjectFailure, EmailBodyFailure);
        }
    }
}