using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.Channel;
using CommonLibrary.Logging;
using CommonLibrary.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QueueFunc01.Context;
using Microsoft.ApplicationInsights.Extensibility;

/* Server Telemetry Channel用
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using Microsoft.ApplicationInsights.Extensibility;
*/

[assembly: FunctionsStartup(typeof(QueueFunc01.Startup))]

namespace QueueFunc01
{
    public class Startup : FunctionsStartup
    {

        //FunctionsStartup
        //NOTE: https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-dotnet-dependency-injection
        //NOTE: https://blog.shibayan.jp/entry/20200823/1598186591
        //NOTE: https://stackoverflow.com/questions/57564396/how-do-i-mix-custom-parameter-binding-with-dependency-injection-in-azure-functio

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
            builder.ConfigurationBuilder
                .SetBasePath(context.ApplicationRootPath)
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }


        public override void Configure(IFunctionsHostBuilder builder)
        {
            string appInsightsKey = builder.GetContext().Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey");
            string logLevel = builder.GetContext().Configuration.GetValue<string>("Log_Level");

            DbContextOptionsBuilder dbContextOptionsBuilder;

            // IFunctionsConfigurationBuilderを別クラスのメソッドで初期化する。
            // ServerTelemetryChannelにするとログがでない。
            builder = TelemetryClientConfigure.ConfigureFunctionsServerTelemetryChannel(builder, appInsightsKey);
            //builder = TelemetryClientConfigure.ConfigureFunctionsInMemoryTelemetryChannel(builder, appInsightsKey);

            builder.Services.AddSingleton<ITelemetryInitializer, PodTelemetryInitializer>();
            builder = myILoggerProvider.Congfigure(builder, appInsightsKey, logLevel);


            builder.Services.AddDbContext<DatabaseContext>(options =>
            {
                //var configuration = provider.GetRequiredService<IConfiguration>();
                dbContextOptionsBuilder = options.UseSqlServer(builder.GetContext().Configuration.GetValue<string>("DB_ConnectionString"));
            });

            /*
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.EnsureCreated();
            }
            */


            //builder.Services.AddApplicationInsightsTelemetry(aiOptions);
            builder.Services.AddSingleton<Functions>();
            builder.Services.BuildServiceProvider(true);

        }
    }
}
