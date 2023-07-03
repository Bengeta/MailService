using Interfaces;
using Listeners;
using MailStuff;
using Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("data/appsettings.json");

builder.Services.AddSingleton<MySmtpServer>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IPhoneRepository, PhoneRepository>();
builder.Services.AddHostedService<RabbitMqListener>();

var app = builder.Build();
var smtpServer = app.Services.GetRequiredService<MySmtpServer>();
smtpServer.RunAsync();

app.Run();