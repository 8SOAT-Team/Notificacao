namespace Notificacao
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Swashbuckle.AspNetCore.Annotations;

    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        // Construtor para injeção de dependência
        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> EnviarEmail([FromBody] EmailRequest request)
        {
            try
            {
                await _emailService.EnviarEmailAsync(request.ToEmail, request.Subject, request.Content);
                return Ok("E-mail enviado com sucesso!");
            }
            catch (Exception ex)
            {
                // Em caso de erro, retornamos uma resposta de erro adequada
                return StatusCode(500, $"Erro ao enviar o e-mail: {ex.Message}");
            }
        }
    }

    public class EmailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}