namespace Notificacao;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    
    [HttpPost]
    public async Task<IActionResult> EnviarEmail([FromBody] EmailRequest request)
    {
        await EmailService.EnviarEmailAsync(request.ToEmail, request.Subject, request.Content);
        return Ok("E-mail enviado com sucesso!");
    }
}

public class EmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}

