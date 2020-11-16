using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using CommonLibrary.Logging;
using Microsoft.Extensions.Logging;
using GwWebApi01.Kafka;

namespace GwWebApi01
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
  
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Application Insightsへのログ出力設定
            //
            // NOTE: https://docs.microsoft.com/ja-jp/azure/azure-monitor/app/asp-net-core#configuration-recommendation-for-microsoftapplicationinsightsaspnetcore-sdk-2150--above
            //
            /*
            services.AddSingleton(typeof(ITelemetryChannel), new ServerTelemetryChannel() { StorageFolder = "./" });
            var telemetryConfigutaion = new TelemetryClientConfigure(Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey")).aiOptions;
            services.AddApplicationInsightsTelemetry(telemetryConfigutaion);
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddApplicationInsights(Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey"));
                loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>
                ("", LogLevel.Error);
                loggingBuilder.AddConsole();
            }); 
            */

            string appInsightsKey = Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey");
            string logLevel = Configuration.GetValue<string>("Log_Level");

            services = TelemetryClientConfigure.ConfigureServerTelemetryChannel(services, appInsightsKey);
            services = myILoggerProvider.Congfigure(services, appInsightsKey, logLevel);


            // シングルトン化。connection refusedが発生した時の例外がハンドルできるかわからず、一旦コメントアウト。
            /*
            string brokerList = Configuration.GetValue<string>("BROKER_LIST");
            string topic = Configuration.GetValue<string>("TOPIC");
            services.AddSingleton(new MyKafkaProducer(brokerList, topic));
            */
            
            services.AddControllers()
            .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
