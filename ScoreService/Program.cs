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
// 配置日誌設置
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss";
    }
);


// 新增 Redis 服務
builder.Services.AddSingleton(_ => ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Value));
// 新增 Grpc 服務
builder.Services.AddGrpc();

var app = builder.Build();

// 註冊 grpc 服務
app.MapGrpcService<ScorerService>();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapGrpcService<ScorerService>().EnableGrpcWeb();
//});

// 確認 Redis 連線
app.Logger.LogInformation(app.Services.GetRequiredService<ConnectionMultiplexer>().IsConnected ? "Redis 連線成功" : "Redis 連線失敗");
app.Logger.LogInformation(app.Services.GetServices<ScorerService>() != null ? "服務註冊成功" : "服務註冊失敗");

app.Run();
