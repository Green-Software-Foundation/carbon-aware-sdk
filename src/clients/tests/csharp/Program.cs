using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using dotenv.net;

// load config
var config = new Config();
DotEnv.Load(new DotEnvOptions(envFilePaths: new string[] { "../.env" }));
if (int.TryParse(Environment.GetEnvironmentVariable("CSHARP_PORT"), out int portA))
{
    config.PORT = portA;
}
else if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out int portB))
{
    config.PORT = portB;
}
else
{
    throw new Exception("PORT must be specified.");
}
config.BASE_URL = Environment.GetEnvironmentVariable("BASE_URL");
if (string.IsNullOrEmpty(config.BASE_URL)) throw new Exception("BASE_URL must be specified.");

// define services
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IConfig>(config);

// define pipeline
var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run($"http://*:{config.PORT}");