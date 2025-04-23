using Amazon.SQS;
using Notificacao;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddHostedService<EmailService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
