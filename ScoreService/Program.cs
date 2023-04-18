using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ScoreService.Services;
using StackExchange.Redis;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Parse("127.0.0.1"), 5010, listenOptions =>
        { listenOptions.Protocols = HttpProtocols.Http2; });
    }).UseKestrel();
// �t�m��x�]�m
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss";
    }
);


// �s�W Redis �A��
builder.Services.AddSingleton(_ => ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Value));
// �s�W Grpc �A��
builder.Services.AddGrpc();

var app = builder.Build();

// ���U grpc �A��
app.MapGrpcService<ScorerService>();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapGrpcService<ScorerService>().EnableGrpcWeb();
//});

// �T�{ Redis �s�u
app.Logger.LogInformation(app.Services.GetRequiredService<ConnectionMultiplexer>().IsConnected ? "Redis �s�u���\" : "Redis �s�u����");
app.Logger.LogInformation(app.Services.GetServices<ScorerService>() != null ? "�A�ȵ��U���\" : "�A�ȵ��U����");

app.Run();
