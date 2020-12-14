using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.Channel;
using CommonLibrary.Logging;
using CommonLibrary.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using System;
using Microsoft.ApplicationInsights.Extensibility;

/* Server Telemetry Channel用
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using Microsoft.ApplicationInsights.Extensibility;
*/

[assembly: FunctionsStartup(typeof(AksPocSampleFunctions.Startup))]

namespace AksPocSampleFunctions
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
            builder.Services.AddWebJobs(x => { return; }).AddKafka(
                (KafkaOptions options) =>{
                    options.MaxBatchSize = builder.GetContext().Configuration.GetValue<int>("KafkaExtension_MaxBatchSize");
                    options.SubscriberIntervalInSeconds = builder.GetContext().Configuration.GetValue<int>("KafkaExtension_SubscriberIntervalInSeconds");
                    options.ExecutorChannelCapacity = builder.GetContext().Configuration.GetValue<int>("KafkaExtension_ExecutorChannelCapacity");
                    options.ChannelFullRetryIntervalInMs = builder.GetContext().Configuration.GetValue<int>("KafkaExtension_ChannelFullRetryIntervalInMs");
                    if (!String.IsNullOrEmpty(builder.GetContext().Configuration.GetValue<string>("Librd_LibkafkaDebug")))
                    {
                        options.LibkafkaDebug = builder.GetContext().Configuration.GetValue<string>("Librd_LibkafkaDebug");
                    }
                });

            string appInsightsKey = builder.GetContext().Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey");
            string logLevel = builder.GetContext().Configuration.GetValue<string>("Log_Level");


            // IFunctionsConfigurationBuilderを別クラスのメソッドで初期化する。
            // ServerTelemetryChannelにするとログがでない。

            builder = TelemetryClientConfigure.ConfigureFunctionsServerTelemetryChannel(builder, appInsightsKey);
            builder.Services.AddSingleton<ITelemetryInitializer, PodTelemetryInitializer>();

            //builder = TelemetryClientConfigure.ConfigureFunctionsInMemoryTelemetryChannel(builder, appInsightsKey);
            builder = myILoggerProvider.Congfigure(builder, appInsightsKey, logLevel);


            //builder.Services.AddApplicationInsightsTelemetry(aiOptions);
　          builder.Services.AddSingleton<Functions>();

            //worker service向け
            //builder.Services.AddApplicationInsightsTelemetryWorkerService(telemetryConfigutaion);
            builder.Services.BuildServiceProvider(true);

  
        }
    }
}
