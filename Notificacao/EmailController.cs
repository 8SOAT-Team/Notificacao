namespace Notificacao;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("send-notification")]
    [SwaggerOperation(Summary = "Enviar uma notificação por e-mail")]
    public async Task<IActionResult> EnviarNotificacao([FromQuery] string para, [FromQuery] string assunto, [FromQuery] string corpo)
    {
        if (string.IsNullOrEmpty(para) || string.IsNullOrEmpty(assunto) || string.IsNullOrEmpty(corpo))
        {
            return BadRequest("Dados inválidos.");
        }

        await _emailService.EnviarEmailAsync(para, assunto, corpo);
        return Ok("E-mail enviado com sucesso!");
    }
}

public class EmailRequest
{
    public string Para { get; set; }
    public string Assunto { get; set; }
    public string Corpo { get; set; }
}

