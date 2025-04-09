using Notificacao;
using SendGrid;

var builder = WebApplication.CreateBuilder(args);

var sendGridApiKey = builder.Configuration.GetValue<string>("SendGrid:ApiKey");
builder.Services.AddSingleton<ISendGridClient>(new SendGridClient(sendGridApiKey));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
