using System.Collections.Concurrent;
using UrlShortener.Database;
using UrlShortener.Endpoints;
using UrlShortener.ExceptionHandling;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Queue to hold pre-created shortenedValues
builder.Services.AddSingleton<ConcurrentQueue<string>>();

builder.Services.AddTransient<IUrlRepository, UrlRepository>();
builder.Services.AddSingleton<IDbConnectionFactory, PostgresConnectionFactory>();

builder.Services.Configure<HostOptions>(opt =>
{
    opt.ServicesStartConcurrently = true;
});
builder.Services.AddHostedService<ShortenedUrlsBackgroundService>();

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