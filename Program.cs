using Interfaces;
using Listeners;
using MailStuff;
using Repository;
using Requests;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("data/appsettings.json");

builder.Services.AddSingleton<MySmtpServer>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IPhoneRepository, PhoneRepository>();
builder.Services.AddSingleton<IMailRepository, MailRepository>();
builder.Services.AddHostedService<RabbitMqListener>();

var app = builder.Build();
var smtpServer = app.Services.GetRequiredService<MySmtpServer>();
smtpServer.RunAsync();

var mail = app.Services.GetRequiredService<IMailRepository>();
var request = new SendVerificationCodeRequest{Mail = "inovozhilov08@gmail.com", Code = "123456"};
mail.SendVerificationCode(request);

app.Run();