using System.Collections.Concurrent;
using RabbitMQ.Client;
using UrlShortener.Database;
using UrlShortener.Endpoints;
using UrlShortener.ExceptionHandling;
using UrlShortener.BackgroundServices;
using UrlShortener.MessageQueue;
using UrlShortener.Caching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder
    .Services
    .RegisterDatabaseAndRepoistories(builder.Configuration)
    .RegisterBackgroundServices(builder.Configuration)
    .RegisterRabbitMq(builder.Configuration)
    .RegisterRedis(builder.Configuration);


var app = builder.Build();

app.UseExceptionHandler(opt => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapUrlEndpoints();

app.Run();